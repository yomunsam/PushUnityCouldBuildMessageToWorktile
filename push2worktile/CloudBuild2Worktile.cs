using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Aliyun.Serverless.Core;
using Aliyun.Serverless.Core.Http;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using System.Net;


namespace Nekonya
{
    public class CloudBuild2Worktile : FcHttpEntrypoint
    {
        private string mAuthor_name;
        private string mAuthor_link;
        private string mAuthor_icon;

        protected override void Init(IWebHostBuilder builder)
        {
            mAuthor_name = Environment.GetEnvironmentVariable("author_name");
            mAuthor_link = Environment.GetEnvironmentVariable("author_link");
            mAuthor_icon = Environment.GetEnvironmentVariable("author_icon");

        }

        public override async Task<HttpResponse> HandleRequest(HttpRequest request, HttpResponse response, IFcContext fcContext)
        {
            //尝试获取环境变量
            //fcContext.Logger.LogInformation("尝试获取环境变量:" + Environment.GetEnvironmentVariable("worktile_url"));
            string worktile_url = Environment.GetEnvironmentVariable("worktile_url");
            if (string.IsNullOrEmpty(worktile_url))
            {
                response.StatusCode = 502;
                fcContext.Logger.LogError("未配置有效的环境变量 worktile_url");
                await response.WriteAsync("not config about worktile url. \n 检查环境变量：worktile_url");
                return response;
            }

            //尝试获取json
            if (request.ContentLength != null && request.ContentLength.Value > 0)
            {
                var stream = request.Body;
                var buffer = new byte[request.ContentLength.Value];
                stream.Read(buffer, 0, buffer.Length);
                string content = Encoding.UTF8.GetString(buffer);

                //fcContext.Logger.LogInformation("读取到json:\n" + content);
                

                //解析
                var json_obj = JsonConvert.DeserializeObject<UnityCloudBuildWebHookJsonModel>(content);


                //先直接尝试发送给worktile
                requestWorktile send_obj;
                if(GetWorktileRequest(json_obj,out send_obj))
                {
                    var json_str = JsonConvert.SerializeObject(send_obj);
                    var json_bytes = Encoding.UTF8.GetBytes(json_str);
                    Post(worktile_url, json_bytes);

                    response.StatusCode = 200;
                    await response.WriteAsync("finish");
                }
                else
                {
                    response.StatusCode = 403;
                    await response.WriteAsync("emmmmm");
                }


            }
            else
            {
                response.StatusCode = 403;
                await response.WriteAsync("error request.");
            }
            


            
            return response;
        }

        private byte[] Post(string url,byte[] post_content)
        {
            
            WebClient client = new WebClient();
            client.Headers.Add("Content-Type", "application/json; charset=utf-8");
            var resp_data = client.UploadData(new Uri(url), "POST", post_content);
            client.Dispose();
            return resp_data;
            
        }

        private bool GetWorktileRequest(UnityCloudBuildWebHookJsonModel unity_json,out requestWorktile request)
        {
            var req = new requestWorktile();
            var att = new SAttachment();

            att.color = "#66ccff";
            if(unity_json.buildStatus == "started") //开始
            {
                att.fallback = "开始云端构建："+ unity_json.buildTargetName;
                att.pretext = "开始云端构建:" + unity_json.buildTargetName;
                att.title = "开始云端构建工程 " + unity_json.projectName;

                if (!string.IsNullOrEmpty(mAuthor_name))
                {
                    att.author_name = mAuthor_name;
                }

                if (!string.IsNullOrEmpty(mAuthor_link))
                {
                    att.author_link = mAuthor_link;
                }

                if (!string.IsNullOrEmpty(mAuthor_icon))
                {
                    att.author_icon = mAuthor_icon;
                }

                att.text = $"开始构建工程：\nTarget:{unity_json.buildTargetName}\n构建号：{unity_json.buildNumber}\n项目名：{unity_json.projectName}\n构建平台：{unity_json.platform}\n任务发起人：{unity_json.startedBy}";
            }
            else if(unity_json.buildStatus == "queued") //加入队列
            {
                att.fallback = "加入云构建队列：" + unity_json.buildTargetName;
                att.pretext = "加入云构建队列:" + unity_json.buildTargetName;
                att.text = "云端构建队列加入任务 " + unity_json.buildTargetName + " | " + unity_json.buildNumber;
            }
            else if (unity_json.buildStatus == "canceled") //被取消
            {
                att.color = "#ff6100";
                att.fallback = "取消构建：" + unity_json.buildTargetName;
                att.pretext = "取消构建:" + unity_json.buildTargetName;
                att.text = "云端构建任务取消 " + unity_json.buildTargetName + " | " + unity_json.buildNumber;

            }
            else if (unity_json.buildStatus == "restarted") //重启
            {
                att.color = "#111111";
                att.fallback = "重启构建：" + unity_json.buildTargetName;
                att.pretext = "重启构建:" + unity_json.buildTargetName;
                att.text = "云端构建任务重启 " + unity_json.buildTargetName + " | " + unity_json.buildNumber;
            }
            else if (unity_json.buildStatus == "success") //成功
            {
                att.fallback = "构建工程完成：" + unity_json.buildTargetName;
                att.pretext = "构建工程完成:" + unity_json.buildTargetName;
                att.title = "构建工程完成 " + unity_json.projectName;
                att.text = $"构建工程完成：\nTarget:{unity_json.buildTargetName}\n构建号：{unity_json.buildNumber}\n项目名：{unity_json.projectName}\n构建平台：{unity_json.platform}\n任务发起人：{unity_json.startedBy}";

                if (!string.IsNullOrEmpty(mAuthor_name))
                {
                    att.author_name = mAuthor_name;
                }

                if (!string.IsNullOrEmpty(mAuthor_link))
                {
                    att.author_link = mAuthor_link;
                }

                if (!string.IsNullOrEmpty(mAuthor_icon))
                {
                    att.author_icon = mAuthor_icon;
                }

                List<Structfields> fields = new List<Structfields>();
                if(!string.IsNullOrEmpty(unity_json.links.share_url.href))
                {
                    att.title_link = unity_json.links.share_url.href;
                    fields.Add(new Structfields()
                    {
                        title = "分享链接",
                        value = unity_json.links.share_url.href
                    });
                }

                if(unity_json.links.artifacts != null)
                {
                    foreach(var item in unity_json.links.artifacts)
                    {
                        if (item.show_download)
                        {
                            foreach (var file in item.files)
                            {
                                if (!string.IsNullOrEmpty(file.filename) && !string.IsNullOrEmpty(file.href))
                                {
                                    fields.Add(new Structfields()
                                    {
                                        title = "[file] " + file.filename,
                                        value = $"md5sum:{file.md5sum}\nsize:{file.size}\nfile url:{file.href}"
                                    });
                                }

                            }
                        }
                        
                    }
                }


                if(fields.Count > 0)
                {
                    att.fields = fields.ToArray();
                }
            }
            else if (unity_json.buildStatus == "failure") //失败
            {
                att.color = "#e3170d";
                att.fallback = "云端构建失败：" + unity_json.buildTargetName;
                att.pretext = "云端构建失败:" + unity_json.buildTargetName;
                att.title = "构建云端工程失败 " + unity_json.projectName;

                if (!string.IsNullOrEmpty(mAuthor_name))
                {
                    att.author_name = mAuthor_name;
                }

                if (!string.IsNullOrEmpty(mAuthor_link))
                {
                    att.author_link = mAuthor_link;
                }

                if (!string.IsNullOrEmpty(mAuthor_icon))
                {
                    att.author_icon = mAuthor_icon;
                }

                att.text = $"构建工程失败：\nTarget:{unity_json.buildTargetName}\n构建号：{unity_json.buildNumber}\n项目名：{unity_json.projectName}\n构建平台：{unity_json.platform}\n任务发起人：{unity_json.startedBy}";
            }
            else
            {
                request = default;
                return false;
            }

            req.attachment = att;
            request = req;
            return true;
        }

    }
}

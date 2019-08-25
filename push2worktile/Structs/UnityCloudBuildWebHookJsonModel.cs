using System;
using System.Collections.Generic;
using System.Text;

namespace Nekonya
{
    [System.Serializable]
    public struct UnityCloudBuildWebHookJsonModel
    {
        /// <summary>
        /// 构建队列中每个平台前面的那个递增的序号
        /// </summary>
        public int buildNumber { get; set; } //
        /// <summary>
        /// 构建状态
        /// </summary>
        public string buildStatus { get; set; }
        /// <summary>
        /// 平台名
        /// </summary>
        public string buildTargetName { get; set; }
        public bool cleanBuild { get; set; }
        public bool credentialsOutdated { get; set; }

        public SLinks links { get; set; }

        public string platform { get; set; }
        public string platformName { get; set; }

        public string projectName { get; set; }

        public string scmType { get; set; }

        public string startedBy { get; set; }

    }

    [System.Serializable]
    public struct SLinks
    {
        public SLink_defalut api_self { get; set; }
        public SLink_defalut api_share { get; set; }

        public SLink_artifacts_Item[] artifacts { get; set; }
        public SLink_defalut dashboard_download { get; set; }
        public SLink_defalut dashboard_download_direct { get; set; }
        public SLink_defalut share_url { get; set; }

    }

    [System.Serializable]
    public struct SLink_defalut
    {
        public string href { get; set; }
        public string method { get; set; }

    }

    [System.Serializable]
    public struct SLink_artifacts_Item
    {
        public SLink_artifacts_Item_files[] files { get; set; }
        public string key { get; set; }
        public string name { get; set; }
        public bool primary { get; set; }

        public bool show_download { get; set; }
    }

    [System.Serializable]
    public struct SLink_artifacts_Item_files
    {
        public string filename { get; set; }
        public string href { get; set; }

        public string md5sum { get; set; }

        public bool resumable { get; set; }
        public long size { get; set; }
    }
}

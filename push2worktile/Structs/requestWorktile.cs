using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Nekonya
{
    [System.Serializable]
    public struct requestWorktile
    {
        public SAttachment attachment { get; set; }
    }

    [System.Serializable]
    public struct SAttachment
    {
        /// <summary>
        /// 用于移动端将提示信息显示在首页上
        /// </summary>
        public string fallback { get; set; }

        public string color { get; set; }

        /// <summary>
        /// 在显示消息正文之前显示的文本内容
        /// </summary>
        public string pretext { get; set; }

        /// <summary>
        /// 作者名
        /// </summary>
        public string author_name { get; set; }

        public string author_link { get; set; }

        public string author_icon { get; set; }

        public string title { get; set; }

        public string title_link { get; set; }

        /// <summary>
        /// 普通文本消息
        /// </summary>
        public string text { get; set; }

        public Structfields[] fields { get; set; }

        
    }


    [System.Serializable]
    public struct Structfields
    {
        public string title { get; set; }

        public string value { get; set; }

        [JsonProperty("short")]
        public int _short { get; set; }

}

}

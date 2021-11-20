namespace RainbowDraw.DATA
{
    public class ItemData
    {
        public string ID { get; set; }
        public int RemainMs { get; set; }
        public bool RemainStartFlg { get; set; } = false;
        public int NotStartMs { get; set; } = 0;
        public bool FadeStartFlg { get; set; } = false;
    }
}

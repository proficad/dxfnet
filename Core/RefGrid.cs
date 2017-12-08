namespace DxfNet
{
    public class RefGridSettings
    {
        public bool Left { get; set; }
        public bool Top { get; set; }
        public bool Right { get; set; }
        public bool Bottom { get; set; }
        public int FieldSize { get; set; }
        public const int GRID_THICKNESS = 50;
        public const int CENTERING_MARK_LEN = 100;
        public const int FIELD_SIZE_MIN = 200;
        public const int FIELD_SIZE_MAX = 2000;
        public const int FIELD_SIZE_DEFAULT = 500;
    }
}
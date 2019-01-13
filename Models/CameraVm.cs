namespace Sample.ShinobiVideo.Lib.Models
{
    public class CameraVm
    {
        public int Id { get; set; }

        public string EquipmentTag { get; set; }

        public string Name { get; set; }

        public string StreamUrl { get; set; }

        public string IpAddress { get; set; }

        public string Brand { get; set; }

        public string Model { get; set; }

        public int? TenantId { get; set; }

        public int LocationId { get; set; }

        public decimal? CoordX { get; set; }

        public decimal? CoordY { get; set; }

        public int? JsmpegPort { get; set; }

        public bool IsDeleted { get; set; }
    }
}
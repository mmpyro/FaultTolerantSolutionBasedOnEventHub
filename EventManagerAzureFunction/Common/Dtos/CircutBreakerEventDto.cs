namespace Common.Dtos
{
    public class CircutBreakerEventDto
    {
        public string Key { get; set; }
        public string Topic { get; set; }
        public object Data { get; set; }
        public string Uri { get; set; }
    }
}

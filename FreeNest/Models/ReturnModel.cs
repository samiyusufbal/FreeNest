namespace FreeNest.Models
{
    public class ReturnModel<T>
    {
        public T Value { get; set; }

        public string ReturnMessage { get; set; }

        public bool ReturnResult { get; set; }
    }
}
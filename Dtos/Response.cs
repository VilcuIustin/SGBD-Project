namespace SGBD_Project.Dtos
{
    public class Response<T>
    {
        public string Message { get; set; }
        public T Data { get; set; }
        public int Status { get; set; } = 200;

        public Response(T Data)
        {
            this.Data = Data;
        }

        public Response(string Message, int Status)
        {
            this.Message = Message;
            this.Status = Status;
        }
    }
}

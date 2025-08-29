using DATA.Enums;
using System.Text;

namespace FreeNest.Helpers
{
    public class ReturnMessage
    {
        public ReturnMessageType MsgType { get; set; } = ReturnMessageType.Error;
        public string? ScriptTxt { get; set; }

        public ReturnMessage CreateMessage(string Title, ReturnMessageType msgType = ReturnMessageType.Error)
        {
            this.MsgType = msgType;

            var script = new StringBuilder();
            script.Append(" $.toast({");
            script.Append(@"text:' " + System.Web.HttpUtility.HtmlEncode(Title) + "',");

            if (msgType == ReturnMessageType.Error)
            {
                script.Append(@" heading:'ERROR',");
                script.Append(" position: 'top-center', loaderBg: '#ff6849' ,  hideAfter: 3500 , icon: 'error' , stack: 6});");
            }
            else
            {
                script.Append(@" heading:'Successful',");
                script.Append(" position: 'top-center', loaderBg: '#ff6849' ,  hideAfter: 3500 , icon: 'success' , stack: 6});");
            }

            this.ScriptTxt = script.ToString();
            return this;
        }
    }

    public class ReturnMessage<T> : ReturnMessage
    {
        public T model { get; set; } = Activator.CreateInstance<T>();
    }
}

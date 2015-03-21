using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Devville.DataService.Web
{
    using Devville.DataService.Contracts;
    using Devville.DataService.Contracts.ServiceResponses;

    public class HelloWorldOperation : IServiceOperation
    {
        public string Name
        {
            get
            {
                return "ShowHelloWorld";
            }
        }

        public string Description
        {
            get
            {
                return "This is a hello world operation";
            }
        }

        public Dictionary<string, string> Parameters
        {
            get
            {
                var parameters = new Dictionary<string, string>();
                parameters["Name"] = "string: This a sample parameter.";
                return parameters;
            }
        }

        public IServiceResponse Execute(HttpContext context)
        {
            var name = context.Request["Name"] ?? "World";
            var response =
                new JsonResponse(
                    new
                        {
                            Message = "Hello " + name,
                            Data = new List<string> { "Hello", name },
                            TimeStamp = DateTime.Now.ToString("O")
                        });

            return response;
        }
    }
}
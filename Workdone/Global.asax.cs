using System.Web.Http;
using Slider.WorkDone.IoC;

namespace Slider.WorkDone.Api
{
	public class WebApiApplication : System.Web.HttpApplication
	{
		private readonly SimpleIoCContainer container= new SimpleIoCContainer();

		protected void Application_Start()
		{
			GlobalConfiguration.Configure(WebApiConfig.Register);
			GlobalConfiguration.Configure(c => container.Register(c));
		}
	}
}

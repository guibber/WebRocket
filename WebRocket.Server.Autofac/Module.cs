using Autofac;
using WebRocket.Server.Wrappers;

namespace WebRocket.Server.Autofac {
  public class Module {
    public void Register(ContainerBuilder builder) {
      builder
        .RegisterType<RocketListener>()
        .As<IRocketListener>()
        .SingleInstance();

      builder
        .RegisterType<HttpListener>()
        .As<IHttpListerner>()
        .SingleInstance();

      builder
        .RegisterType<RocketAcceptor>()
        .As<IRocketAcceptor>()
        .SingleInstance();
    }
  }
}
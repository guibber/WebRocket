using Autofac;
using WebRocket.Server.Wrappers;

namespace WebRocket.Server.Autofac {
  public class Module {
    public void Register(ContainerBuilder builder) {
      Register<NoOpObserver>(builder);
    }

    public void Register<T>(ContainerBuilder builder) where T : IObserver {
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

      builder
        .RegisterType<T>()
        .As<IObserver>()
        .SingleInstance();
    }
  }
}
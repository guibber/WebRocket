using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebRocket.Client {
  public interface IClientRocket : IRocket{
    Task ConnectAsync(Uri uri, CancellationToken token);
  }
}
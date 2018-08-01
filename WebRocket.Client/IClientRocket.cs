using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebRocket.Client {
  public interface IClientRocket : IRocket{
    Task<bool> ConnectAsync(Uri uri, CancellationToken token);
  }
}
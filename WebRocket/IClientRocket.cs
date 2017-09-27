using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WebRocket {
  public interface IClientRocket : IRocket{
    Task<bool> ConnectAsync(Uri uri, CancellationToken token);
  }
}
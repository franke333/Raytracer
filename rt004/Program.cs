using Util;
using System.CommandLine;

namespace rt004;

internal class Program
{

  static void Main(string[] args)
  {
        new FrontController().Invoke(args);
  }
}

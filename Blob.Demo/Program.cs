using Blob.Demo.Service;
using Blob.Demo.Service.Contract;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Hello, World!");

var services = new ServiceCollection();

services.AddTransient<IBlobService, BlobService>();
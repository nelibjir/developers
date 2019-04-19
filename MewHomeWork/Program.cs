using System;
using System.Collections.Generic;
using MewHomeWork.Services.ExchangeRateProvider;
using MewHomeWork.Services.Models;

namespace MewHomeWork
{
  class Program
  {
    static void Main(string[] args)
    {
      ExchangeRateProvider provider = new ExchangeRateProvider();

      Currency[] currencies = { new Currency("USD"), new Currency("AUD"), new Currency("RUB"), new Currency("BRL") };
      IEnumerable<ExchangeRate> rates = provider.GetExchangeRates(currencies);

      foreach (ExchangeRate rate in rates)
        Console.WriteLine("Exchange rate {0} has value {1}", rate.ExchangeName, rate.Rate);

      Console.ReadLine();
    }
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using MewHomeWork.Services.Externals.Clients;
using MewHomeWork.Services.Models;

namespace MewHomeWork.Services.ExchangeRateProvider
{
  public class ExchangeRateProvider
  {
    // could be used as fallback
    private const string _OtherCurrenciesUrl = "https://www.cnb.cz/cs/financni-trhy/devizovy-trh/kurzy-ostatnich-men/kurzy-ostatnich-men/kurzy.xml";

    private const string _MainCurrenciesUrl = "https://www.cnb.cz/cs/financni_trhy/devizovy_trh/kurzy_devizoveho_trhu/denni_kurz.xml";

    private const string _XmlRootAttributeName = "tabulka";
    private const string _XmlRowAttributeName = "radek";
    private const string _XmlCodeAttributName = "kod";
    private const string _XmlRateAttributeName = "kurz";
    private const string _XmlAmountAttributeName = "mnozstvi";

    public IEnumerable<ExchangeRate> GetExchangeRates(IEnumerable<Currency> currencies)
    {
      if (currencies == null)
        throw new ArgumentNullException(nameof(currencies));
      if (!currencies.Any())
        Enumerable.Empty<ExchangeRate>();

      string data = (new HttpClient().GetDataFromSourceAsync(_MainCurrenciesUrl, default(CancellationToken))).Result;

      return CreateRates(currencies, XmlToDictionary(data));
    }

    private IEnumerable<ExchangeRate> CreateRates(IEnumerable<Currency> currencies, IDictionary<string, double> CurrencyToRate)
    {
      List<Currency> currenciesList = currencies.Select(a => new Currency(a.Code.ToUpper())).ToList();
      List<ExchangeRate> result = new List<ExchangeRate>();
      for (int i = 0; i < currenciesList.Count - 1; i++)
      {
        string currencyNameFrom = currenciesList[i].Code;
        if (!CurrencyToRate.TryGetValue(currencyNameFrom, out double rateFrom))
          continue;
        for (int k = i + 1; k < currenciesList.Count; k++)
        {
          string currencyNameTo = currenciesList[k].Code;
          if (!CurrencyToRate.TryGetValue(currencyNameTo, out double rateTo))
            continue;

          result.Add(new ExchangeRate
          {
            ExchangeName = currencyNameFrom + "/" + currencyNameTo,
            Rate = rateFrom / rateTo
          });
        }
      }

      return result;
    }

    private IDictionary<string, double> XmlToDictionary(string data)
    {
      XElement rootElement = XElement.Parse(data);
      IEnumerable<string> currencyName = rootElement
        .Elements(_XmlRootAttributeName)
        .Elements(_XmlRowAttributeName)
        .Select(n => n.Attribute(_XmlCodeAttributName).Value);
      IEnumerable<double> normalizedRateToCzk = rootElement
        .Elements(_XmlRootAttributeName)
        .Elements(_XmlRowAttributeName)
        .Select(v =>
        {
          double exchangeRate = double.Parse(v.Attribute(_XmlRateAttributeName).Value);
          int amount = int.Parse(v.Attribute(_XmlAmountAttributeName).Value);
          return exchangeRate / amount;
        }
        );

      return currencyName
        .Zip(normalizedRateToCzk, (k, v) => new { k, v })
        .ToDictionary(item => item.k, item => item.v);
    }
  }
}

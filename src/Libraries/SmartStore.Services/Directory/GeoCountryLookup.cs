using System;
using System.Net;
using MaxMind.GeoIP2;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Utilities;
using SmDir = SmartStore.Core.Domain.Directory;

namespace SmartStore.Services.Directory
{
    public partial class GeoCountryLookup : DisposableObject, IGeoCountryLookup
    {
        private readonly DatabaseReader _reader;
        private readonly object _lock = new object();

        public GeoCountryLookup()
        {
            _reader = new DatabaseReader(CommonHelper.MapPath("~/App_Data/GeoLite2/GeoLite2-Country.mmdb"));
        }

        public LookupCountryResponse LookupCountry(string addr)
        {
            if (addr.HasValue() && IPAddress.TryParse(addr, out var ipAddress))
            {
                return LookupCountry(ipAddress);
            }

            return null;
        }

        public LookupCountryResponse LookupCountry(IPAddress addr)
        {
            Guard.NotNull(addr, nameof(addr));

            if (_reader.TryCountry(addr, out var response) && response.Country != null)
            {
                var country = response.Country;
                return new LookupCountryResponse
                {
                    GeoNameId = country.GeoNameId,
                    IsoCode = country.IsoCode,
                    Name = country.Name,
                    IsInEu = country.IsInEuropeanUnion
                };
            }

            return null;
        }

        protected override void OnDispose(bool disposing)
        {
            if (disposing && _reader != null)
            {
                _reader.Dispose();
            }
        }
    }
}
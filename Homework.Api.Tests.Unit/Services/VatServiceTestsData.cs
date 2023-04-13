using System.Collections;
using Homework.Api.Models;

namespace Homework.Api.Tests.Unit.Services;

public class VatServiceTestsData : IEnumerable<object[]>
{
    private static IEnumerable<object[]> _testCalculateData =>
        new List<object[]>
        {
            new object[] { new VatRequestInput(1000, null, null, 20), new VatCalculationResponse(1000, 833.33m, 166.67m) },
            new object[] { new VatRequestInput(1000, null, null, 13), new VatCalculationResponse(1000, 884.96m, 115.04m) },
            new object[] { new VatRequestInput(1000, null, null, 10), new VatCalculationResponse(1000, 909.09m, 90.91m) },
            new object[] { new VatRequestInput(null, 500, null, 20), new VatCalculationResponse(600, 500, 100) },
            new object[] { new VatRequestInput(null, 500, null, 13), new VatCalculationResponse(565, 500, 65) },
            new object[] { new VatRequestInput(null, 500, null, 10), new VatCalculationResponse(550, 500, 50) },
            new object[] { new VatRequestInput(null, null, 150, 20), new VatCalculationResponse(900, 750, 150) },
            new object[] { new VatRequestInput(null, null, 150, 13), new VatCalculationResponse(1303.85m, 1153.85m, 150) },
            new object[] { new VatRequestInput(null, null, 150, 10), new VatCalculationResponse(1650, 1500, 150) }
        };
    
        public IEnumerator<object[]> GetEnumerator()
        {
            return _testCalculateData.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
}
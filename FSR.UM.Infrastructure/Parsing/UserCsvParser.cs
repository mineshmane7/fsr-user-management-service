using FSR.UM.Core.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using CsvHelper;

namespace FSR.UM.Infrastructure.Parsing
{
    public static class UserCsvParser
    {
        public static List<CreateUserRequest> Parse(Stream stream)
        {
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            return csv.GetRecords<CreateUserRequest>().ToList();
        }
    }
}

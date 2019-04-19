namespace Common.Infrastructure
{
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Newtonsoft.Json;
    using RestSharp;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class RestRequestHelpers
    {
        private const int DefaultLimit = 100;
        private const int MaximumLimit = 10000;

        public static IRestRequest AddSorting(
            this IRestRequest request,
            string sort,
            Dictionary<string, string> sortMapping)
        {
            var sortHeader = sort.CreateSortObject(sortMapping);

            if (!string.IsNullOrWhiteSpace(sortHeader))
                request.AddHeader(AddSortingExtension.HeaderName, sortHeader);

            return request;
        }

        public static IRestRequest AddPagination(
            this IRestRequest request,
            int? offset,
            int? limit)
        {
            offset = offset ?? 0;
            limit = limit ?? DefaultLimit;

            if (offset <= 0)
                offset = 0;

            if (limit > MaximumLimit)
                limit = MaximumLimit;

            request.AddHeader(AddPaginationExtension.HeaderName, $"{offset},{limit}");

            return request;
        }

        public static IRestRequest AddFiltering(
            this IRestRequest request,
            object filter)
        {
            if (filter != null)
                request.AddHeader(ExtractFilteringRequestExtension.HeaderName, JsonConvert.SerializeObject(filter));

            return request;
        }

        private static string CreateSortObject(
            this string sort,
            Dictionary<string, string> sortMapping)
        {
            if (string.IsNullOrWhiteSpace(sort))
                return string.Empty;

            var sortPieces = sort.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var sortField = sortPieces.First().ToLowerInvariant();

            var originalSortField = sortField.StartsWith("-")
               ? sortField.Substring(1)
               : sortField;

            var normalisedSortMapping = sortMapping.ToDictionary(x => x.Key.ToLowerInvariant(), x => x.Value.ToLowerInvariant());

            if (!normalisedSortMapping.ContainsKey(originalSortField))
                return string.Empty;

            return sortField.StartsWith("-")
                ? $"descending,{normalisedSortMapping[originalSortField]}"
                : $"ascending,{normalisedSortMapping[originalSortField]}";
        }
    }
}

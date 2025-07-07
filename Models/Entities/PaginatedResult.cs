namespace StackOverFlowClone.Models.Entities
{
    public class PaginatedResult<T>
    {

        private IEnumerable<T> _Items { get; set; }
        public int TotalCount { get; set; }

        public PaginatedResult(IEnumerable<T> items)
        {

            _Items = items;

        }
        public Task<IEnumerable<T>> GetPageItems(int pageNumber , int pageSize)
        {
            var results = _Items
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Task.FromResult<IEnumerable<T>>(results);
        }
    }
}

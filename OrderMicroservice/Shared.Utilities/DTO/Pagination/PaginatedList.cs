namespace Shared.Utilities.DTO.Pagination
{
    public class PaginatedList<T>
    {
        public PaginatedList(IReadOnlyList<T> items, PaginationMetaData meta)
        {
            Items = items;
            Pagination = meta;
        }
        public IReadOnlyList<T> Items { get; }
        public PaginationMetaData Pagination { get; }
    }
    public class PaginatedListVM<T>
    {
        public PaginatedListVM(T items, PaginationMetaData meta)
        {
            Items = items;
            Pagination = meta;
        }
        public T Items { get; }
        public PaginationMetaData Pagination { get; }
    }
}

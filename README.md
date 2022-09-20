# KeySetPaginator
![image](https://user-images.githubusercontent.com/13892385/189951522-0bd23515-468d-4ccc-9fa5-3bcb2475c542.png)

To support an easy way to paginate result in EF CORE, I've implemented this library.

for more info on pagination and keyset pagination (Also known as cursor pagination) read [here](https://docs.microsoft.com/en-us/ef/core/querying/pagination): 


Learn about why the standard offset based pagination (`Take().Skip()`) is bad [here](http://use-the-index-luke.com/no-offset).

The extension method allows to easily **skip**, **order by**, and **get all results** (while using key set pagination).

## Usage

To use the library at package reference:
    <PackageReference Include="KeySetPaginator" Version="DesiredVersion" />


On your search action use: 
```cs
KeySetPaginator.Queryable.Paginator
-----------------------------------
Query = Query.AddSorting(pagingRequest.KeySetToken, pagingRequest.SortDirection);
```
This will use the Query (of type queryable), to sort it, by KeySetToken, by SortDirection (Ascending, Descending)

```cs
KeySetPaginator.Queryable.Paginator
-----------------------------------
Query = Query.KeySetSkip(pagingRequest.KeySetToken, pagingRequest.SortDirection);
```
This will use the Query (of type queryable), to skip relevant rows from KeySetToken, by SortDirection (Ascending, Descending)

NOTE - in order to the sorting and skipping to work properly they need to have matching params (keySetToken, direction)

```cs
KeySetPaginator.PaginatorAPI
-----------------------------
await PaginatorAPI.GetAllResults(
                SearchActionExample,
                new KeySetPagingRequest<ExampleToken, ExampleRequest>
                {
                    PageSize = 3,
                    SortDirection = SortDirection.asc,
                    KeySetToken = new ExampleToken() { StringName = KeySetToken.InitField("sharon2"), NullableName = KeySetToken.InitField(2M) },
                    Request = new ExampleRequest()
                });
```
This will get all the results in a loop, in the loop it will call SearchActionExample, 
with the relevant filtering request (ExampleRequest), page size, direction and starting token (ExampleToken).

so that it will get all the results filtered by request after the current token (offset).

There is also post filtering override to the above method:
```cs
KeySetPaginator.PaginatorAPI
-----------------------------
await PaginatorAPI.GetAllResults(
                SearchActionExample,
                AfterSearchFalse,
                new KeySetPagingRequest<ExampleToken, ExampleRequest>
                {
                    PageSize = 1,
                    SortDirection = SortDirection.asc,
                    KeySetToken = new ExampleToken() { StringName = KeySetToken.InitField("sharon2"), NullableName = KeySetToken.InitField(3M) },
                    Request = new ExampleRequest()
                });
```
Here we have AfterSearchFalse that will return false after the first search action.
It means it will get only the first "PageSize" (1) results after the token.


## Data Structures

```cs
 public class ExampleToken : KeySetToken
    {
        public ExampleToken()
            : base(new List<string>() { nameof(StringName), nameof(NullableName)})
        { }
        public ExampleToken(List<string> DefaultFields)
            : base(DefaultFields ?? new List<string>() { nameof(StringName), nameof(NullableName) })
        { }

        public KeySetTokenValue<string> StringName { get; set; }
        public KeySetTokenValue<decimal> DecimalName { get; set; }
        public KeySetTokenValue<int> IntName { get; set; }
        public KeySetTokenValue<long> LongName { get; set; }
        public KeySetTokenValue<decimal> NullableName { get; set; }
        public override List<string> DefaultFields { get; set; }
    }
```

***KeySetToken*** - A representation of the token depending on the model, defined in the relevant order.

Those deriving from it can add fields (the fields must be of type KeySetTokenValue<FieldType>).
"FieldType" is the type of the field which is in db (non-nullable).
Each field defined and initialized will be sorted and skipped only if its defined and initialized (according to the order in the class).

              *Note*: The fields names must be the exact as in the model/DB.

***KeySetTokenValue*** - A wrapper to a token value.

***SortDirectionDTO*** - A enum for sorting Ascending/Descending.

***KeySetPagingRequest*** - A Paging Request options object for when using PagintorAPI.

used to define the request, the token, sort direction, page size, and timeout (if needed by the search action).

## Important Notes
* Its imprtant to define in the keyset token a combination of the fields the is unique as explained [here](https://docs.microsoft.com/en-us/ef/core/querying/pagination)
* As with any other query, proper indexing is vital for good performance: make sure to have indexes in place which correspond to your pagination ordering. If ordering by more than one column, an index over those multiple columns can be defined; this is called a composite index.
For more information, [see the documentation page on indexes](https://docs.microsoft.com/en-us/ef/core/modeling/indexes).

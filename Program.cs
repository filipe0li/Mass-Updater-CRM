using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Messages;
using System;

public class Program
{
    public static void Main()
    {
        try
        {
            var query = GetEntities(QueryAccounts());
            /*
            // Mock Test
            var query = new EntityCollection();
            var entity = new Entity("account", new Guid("00000000-0000-0000-0000-000000000000"));
            query.Entities.Add(entity);
            */
            int packageSize = 50;
            int loop = (int)Math.Ceiling((float)query.Entities.Count / packageSize);
            for (int i = 0; i < loop; i++)
            {
                var update = BuildAccount(query, packageSize, i);
                UpdateCRM(update);
                Console.WriteLine($"Pacote de entidades nº: {i} importado para {query.EntityName}!");
            }
            Console.WriteLine("Finalizado!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro: " + ex);
        }
        Console.ReadKey();
    }
    protected static EntityCollection BuildAccount(EntityCollection query, int packageSize, int count)
    {
        count *= packageSize;
        var entityCollection = new EntityCollection();
        for (int i = 0; i < packageSize; i++)
        {
            if (count == query.Entities.Count) { break; }
            var entity = new Entity("account", query.Entities[count].Id);
            entity.Attributes["name"] = "updeted field";
            entityCollection.Entities.Add(entity);
            count++;
        }
        return entityCollection;
    }
    protected static QueryExpression QueryAccounts()
    {
        // USE XrmToolBox - FetchXML Buider - View => QueryExpression
        var query_0_name = "test";
        var query_0_accountid = "00000000-0000-0000-0000-000000000000";

        var query = new QueryExpression("account");
        query.ColumnSet.AddColumns("accountid");

        var query_Criteria_0 = new FilterExpression(LogicalOperator.Or);
        query_Criteria_0.AddCondition("name", ConditionOperator.Equal, query_0_name);
        query_Criteria_0.AddCondition("accountid", ConditionOperator.Equal, query_0_accountid);
        query.Criteria.AddFilter(query_Criteria_0);

        return query;
    }
    protected static EntityCollection GetEntities(QueryExpression query)
    {
        var entities = new EntityCollection();
        query.PageInfo = new PagingInfo();
        query.PageInfo.PageNumber = 1;
        bool moreData;
        do
        {
            EntityCollection result = service.RetrieveMultiple(query);
            entities.Entities.AddRange(result.Entities);
            moreData = result.MoreRecords;
            query.PageInfo.PageNumber++;
            query.PageInfo.PagingCookie = result.PagingCookie;
        } while (moreData);
        return entities;
    }
    protected static void UpdateCRM(EntityCollection input)
    {
        var request = new ExecuteMultipleRequest()
        {
            Settings = new ExecuteMultipleSettings()
            {
                ContinueOnError = false,
                ReturnResponses = true
            },
            Requests = new OrganizationRequestCollection()
        };
        foreach (var entity in input.Entities)
        {
            var updateRequest = new UpdateRequest { Target = entity };
            request.Requests.Add(updateRequest);
        }
        ExecuteMultipleResponse response = (ExecuteMultipleResponse)service.Execute(request);
        int count = 0;
        foreach (var item in response.Responses)
        {
            if (item.Fault != null)
            {
                Console.WriteLine($"ERRO na entidade nº: {count}!\n{item.Fault}");
            }
            else if (item.Response.Results.Count > 0)
            {
                Console.WriteLine((Guid)item.Response.Results["id"]);
            }
            count++;
        }
        Console.WriteLine($"{count} entidades atualizadas no CRM!");
    }
}

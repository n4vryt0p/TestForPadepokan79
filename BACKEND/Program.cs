using BACKEND.DATA;
using BACKEND.DATA.Models;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContextPool<DataContext>((sp, options) =>
    {
        _ = options.UseSqlServer(connectionString, b =>
        {
            _ = b.CommandTimeout(3600);
            _ = b.EnableRetryOnFailure(2);
        });
    });


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();


using var scope = app.Services.CreateScope();
var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
await dataContext.Database.EnsureCreatedAsync();

#region Minimal API

    #region Account API
    app.MapGet("/accountlist", async (DataContext db) => await db.Accounts.AsNoTracking().ToListAsync())
        .WithTags("Account")
        .WithName("AccountList")
        .WithOpenApi();

    _ = app.MapPost("/account/add", async (UserDto userdto, DataContext db) =>
    {
        await db.Accounts.AddAsync(new Account{Name = userdto.Name});
        await db.SaveChangesAsync();
        return Results.Ok();
    })
    .WithTags("Account")
    .WithName("AddAccount")
    .WithOpenApi();

    _ = app.MapPut("/account/Edit", async (UserDto userdto,DataContext db) =>
    {
        var isHere = await db.Accounts.FirstOrDefaultAsync(t => t.Id == userdto.Id);
        if (isHere == null) return Results.NotFound();

        isHere.Name = userdto.Name;
        db.Accounts.Update(isHere);
        await db.SaveChangesAsync();
        return Results.Ok();
    })
    .WithTags("Account")
    .WithName("EditAccount")
    .WithOpenApi();

    _ = app.MapDelete("/account/del/{accountId}", async (int accountId, DataContext db) =>
    {
        var isHere = await db.Accounts.FirstOrDefaultAsync(t => t.Id == accountId);
        if (isHere == null) return Results.NotFound();

        db.Accounts.Remove(isHere);
        await db.SaveChangesAsync();
        return Results.Ok();
    })
    .WithTags("Account")
    .WithName("DeleteAccount")
    .WithOpenApi();
    #endregion

    #region Master Transaction
    app.MapGet("/masterlist", async (DataContext db) => await db.MasterTransactions.AsNoTracking().ToListAsync())
        .WithTags("Master")
        .WithName("MasterList")
        .WithOpenApi();

    _ = app.MapPost("/master/add", async (MasterDto masterDto, DataContext db) =>
    {
        await db.MasterTransactions.AddAsync(new MasterTransaction {Name = masterDto.Name});
        await db.SaveChangesAsync();
        return Results.Ok();
    })
    .WithTags("Master")
    .WithName("AddMaster")
    .WithOpenApi();

    _ = app.MapPut("/master/Edit", async (MasterDto masterDto, DataContext db) =>
    {
        var isHere = await db.MasterTransactions.FirstOrDefaultAsync(t => t.Id == masterDto.Id);
        if (isHere == null) return Results.NotFound();

        isHere.Name = masterDto.Name;
        db.MasterTransactions.Update(isHere);
        await db.SaveChangesAsync();
        return Results.Ok();
    })
    .WithTags("Master")
    .WithName("EditMaster")
    .WithOpenApi();

    _ = app.MapDelete("/master/del/{masterId}", async (int masterId, DataContext db) =>
    {
        var isHere = await db.MasterTransactions.FirstOrDefaultAsync(t => t.Id == masterId);
        if (isHere == null) return Results.NotFound();

        db.MasterTransactions.Remove(isHere);
        await db.SaveChangesAsync();
        return Results.Ok();
    })
    .WithTags("Master")
    .WithName("DeleteMaster")
    .WithOpenApi();
    #endregion

    #region Transactions
    app.MapGet("/transactionlist", async (DataContext db) => await db.Transactions.AsNoTracking().Select(t => new TransDto(
                t.Id,t.AccountId,null,t.TransactionDate, t.MasterTransactionId, t.DebitCreditStatus,t.Amount, t.Point
            )).ToListAsync())
        .WithTags("Transaction")
        .WithName("TransactionList")
        .WithOpenApi();

    _ = app.MapPost("/transaction/add", async (TransDto transDto, DataContext db) =>
        {
            await db.Transactions.AddAsync(new Transaction
            {
                AccountId = transDto.AccountId,
                TransactionDate = DateTime.Now,
                MasterTransactionId = transDto.MasterTransactionId,
                DebitCreditStatus = transDto.DebitCreditStatus,
                Amount = transDto.Amount
            });
            await db.SaveChangesAsync();
            return Results.Ok();
        })
        .WithTags("Transaction")
        .WithName("AddTransaction")
        .WithOpenApi();

    _ = app.MapPut("/transaction/Edit", async (TransDto transDto, DataContext db) =>
    {
        var isHere = await db.Transactions.FirstOrDefaultAsync(t => t.Id == transDto.Id);
        if (isHere == null) return Results.NotFound();

        isHere.AccountId = transDto.AccountId;
        isHere.TransactionDate = DateTime.Now;
        isHere.MasterTransactionId = transDto.MasterTransactionId;
        isHere.DebitCreditStatus = transDto.DebitCreditStatus;
        isHere.Amount = transDto.Amount;
        db.Transactions.Update(isHere);
        await db.SaveChangesAsync();
        return Results.Ok();
    })
    .WithTags("Transaction")
    .WithName("EditTransaction")
    .WithOpenApi();

    _ = app.MapDelete("/transaction/del/{transId}", async (int transId, DataContext db) =>
    {
        var isHere = await db.Transactions.FirstOrDefaultAsync(t => t.Id == transId);
        if (isHere == null) return Results.NotFound();

        db.Transactions.Remove(isHere);
        await db.SaveChangesAsync();
        return Results.Ok();
    })
    .WithTags("Transaction")
    .WithName("DeleteTransaction")
    .WithOpenApi();
#endregion

    #region DropdownList
    app.MapGet("/ddl", async (DataContext db) =>
        {
            var uList = await db.Accounts.AsNoTracking().Select(rr => new ListDdl { Id = rr.Id, Text = rr.Name }).ToListAsync();
            var mList = await db.MasterTransactions.AsNoTracking().Select(rr => new ListDdl { Id = rr.Id, Text = rr.Name }).ToListAsync();

            return new AllDdl(uList, mList);
        })
    .WithTags("Dropdown")
    .WithName("DropdownUserAndMaster")
    .WithOpenApi();

app.MapGet("/ddlreport", async (DataContext db) =>
    {
        var uList = await db.Accounts.AsNoTracking().Select(rr => new ListDdl { Id = rr.Id, Text = rr.Name }).ToListAsync();
        var mList = await db.MasterTransactions.AsNoTracking().Select(rr => new ListDdl { Id = rr.Id, Text = rr.Name }).ToListAsync();
        uList.Insert(0, new ListDdl { Id = 0, Text = "All" });
        return new AllDdl(uList, mList);
    })
    .WithTags("Dropdown")
    .WithName("DropdownUserAndMasterRepport")
    .WithOpenApi();
#endregion

#region Reports
app.MapGet("/reportpoints", async (DataContext db) =>
        await db.Transactions.AsNoTracking().GroupBy(tt => tt.AccountId).Select(rr => new TransDto(
            null,rr.Key,null,null,null,null,null,rr.Sum(d => d.Point)
            )).ToListAsync()
    )
    .WithTags("Report")
    .WithName("ReportPoints")
    .WithOpenApi();

app.MapGet("/reportall", async (DataContext db) =>
        await db.Transactions.AsNoTracking().Select(rr => new TransDto(
            null, rr.AccountId, rr.Account.Name, rr.TransactionDate, rr.MasterTransactionId, rr.DebitCreditStatus, rr.Amount, null
        )).ToListAsync()
    )
    .WithTags("Report")
    .WithName("ReportAll")
    .WithOpenApi();
#endregion

#endregion


app.Run();

#region DTO

internal record UserDto(int? Id, string? Name);

internal record MasterDto(int? Id, string? Name);

internal record TransDto(int? Id, int? AccountId, string? AccountName, DateTime? TransactionDate, int? MasterTransactionId, string? DebitCreditStatus, long? Amount, long? Points);

internal record ReportDto(int? AccountId, DateTime FromDate, DateTime ToDate);

internal record AllDdl(IEnumerable<ListDdl>? UserList, IEnumerable<ListDdl>? MasterList);

public class ListDdl
{
    public int Id { get; set; }
    public string? Text { get; set; }
}

#endregion
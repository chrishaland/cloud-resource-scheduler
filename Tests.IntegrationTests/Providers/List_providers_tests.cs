﻿using Service.Providers;

namespace Tests.IntegrationTests.Providers;

[TestFixture]
public class List_providers_tests
{
    [Test]
    public async Task Return_list_of_all_providers()
    {
        await SUT.Database.DeleteAllAsync<Provider>();
        var entity1 = new AzureProvider { Name = "dev", ClientId = "id1", ClientSecret = "secret", SubscriptionId = Guid.NewGuid().ToString(), TenantId = Guid.NewGuid().ToString() };
        var entity2 = new AzureProvider { Name = "dev", ClientId = "id2", ClientSecret = "secret", SubscriptionId = Guid.NewGuid().ToString(), TenantId = Guid.NewGuid().ToString() };
        await SUT.Database.Providers.AddRangeAsync(entity1, entity2);
        await SUT.Database.SaveChangesAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/provider/list");

        var (response, content) = await SUT.SendHttpRequest<ListProvidersResponse>(request, new { });

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(content?.Providers.Length, Is.EqualTo(2));

            var dto1 = content?.Providers.Single(e => e.Id.Equals(entity1.Id));
            Assert.That(dto1?.Id, Is.EqualTo(entity1.Id));
            Assert.That(dto1?.Name, Is.EqualTo(entity1.Name));

            var dto2 = content?.Providers.Single(e => e.Id.Equals(entity2.Id));
            Assert.That(dto2?.Id, Is.EqualTo(entity2.Id));
            Assert.That(dto2?.Name, Is.EqualTo(entity2.Name));
        });
    }

    [Test]
    public async Task Return_empty_list_when_no_providers()
    {
        await SUT.Database.DeleteAllAsync<Provider>();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/provider/list");

        var (response, content) = await SUT.SendHttpRequest<ListProvidersResponse>(request, new { });

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(content?.Providers, Is.Not.Null);
            CollectionAssert.IsEmpty(content?.Providers);
        });
    }
}

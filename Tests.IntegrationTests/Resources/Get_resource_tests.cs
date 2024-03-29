﻿using Service.Resources;

namespace Tests.IntegrationTests.Resources;

[TestFixture]
public class Get_resource_tests
{
    [Test]
    public async Task Get_existing_virtual_machine()
    {
        var provider = await SUT.Database.Providers.AddAsync(new Provider { Name = "dev" });
        var environment = await SUT.Database.Environments.AddAsync(new Environment { Name = "env" });
        var entity = new VirtualMachine { Name = "dev", ResourceGroup = "rg", Description = "Development", Provider = provider.Entity, Environments = new List<Environment> { environment.Entity } };
        await SUT.Database.VirtualMachines.AddAsync(entity);
        await SUT.Database.SaveChangesAsync();

        var data = new { id = entity.Id };
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/resource/get");

        var (response, content) = await SUT.SendHttpRequest<GetResourceResponse>(request, data);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        Assert.Multiple(() =>
        {
            Assert.That(content?.Resource.Id, Is.EqualTo(entity.Id));
            Assert.That(content?.Resource.Name, Is.EqualTo(entity.Name));
            Assert.That(content?.Resource.Description, Is.EqualTo(entity.Description));
            Assert.That(content?.Resource.ResourceGroup, Is.EqualTo(entity.ResourceGroup));
            Assert.That(content?.Resource.ProviderId, Is.EqualTo(provider.Entity.Id));
            CollectionAssert.Contains(content?.Resource.EnvironmentIds, environment.Entity.Id);
        });
    }

    [Test]
    public async Task Get_existing_virtual_machine_scale_set()
    {
        var provider = await SUT.Database.Providers.AddAsync(new Provider { Name = "dev" });
        var environment = await SUT.Database.Environments.AddAsync(new Environment { Name = "env" });
        var entity = new VirtualMachineScaleSet { Name = "dev", ResourceGroup = "rg", Description = "Development", Capacity = 10, Provider = provider.Entity, Environments = new List<Environment> { environment.Entity } };
        await SUT.Database.VirtualMachineScaleSets.AddAsync(entity);
        await SUT.Database.SaveChangesAsync();

        var data = new { id = entity.Id };
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/resource/get");

        var (response, content) = await SUT.SendHttpRequest<GetResourceResponse>(request, data);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        Assert.Multiple(() =>
        {
            Assert.That(content?.Resource.Id, Is.EqualTo(entity.Id));
            Assert.That(content?.Resource.Name, Is.EqualTo(entity.Name));
            Assert.That(content?.Resource.Description, Is.EqualTo(entity.Description));
            Assert.That(content?.Resource.ResourceGroup, Is.EqualTo(entity.ResourceGroup));
            Assert.That(content?.Resource.VirtualMachineScaleSetExtentions?.Capacity, Is.EqualTo(entity.Capacity));
        });
    }

    [TestCase("00000000-0000-0000-0000-000000000000")]
    [TestCase("d982041a-3789-40ea-909d-479386000602")]
    public async Task Getting_non_existent_resources_results_in_not_found_response(string id)
    {
        var data = new { id = Guid.Parse(id) };
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/resource/get");

        var (response, _) = await SUT.SendHttpRequest<GetResourceResponse>(request, data);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}

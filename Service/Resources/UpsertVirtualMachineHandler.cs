﻿namespace Service.Resources;

public class UpsertVirtualMachineHandler
{
    private readonly DatabaseContext _context;

    public UpsertVirtualMachineHandler(DatabaseContext context)
    {
        _context = context;
    }

    internal async Task<Guid> Create(UpsertResourceRequest request, List<Environment> environments, CancellationToken ct)
    {
        var resource = new VirtualMachine
        {
            Name = request.Name,
            Description = request.Description,
            ResourceGroup = request.ResourceGroup,
            ProviderId = request.ProviderId
        };

        if (environments?.Any() == true)
        {
            resource.Environments = environments;
        }

        var entity = await _context.VirtualMachines.AddAsync(resource, ct);
        await _context.SaveChangesAsync(ct);
        return entity.Entity.Id;
    }

    internal async Task<Guid> Update(UpsertResourceRequest request, List<Environment> environments, CancellationToken ct)
    {
        if (!request.Id.HasValue) return Guid.Empty;

        var vmss = await _context.VirtualMachines
            .Include(v => v.Environments)
            .SingleAsync(v => v.Id.Equals(request.Id.Value), ct);

        vmss.Name = request.Name;
        vmss.Description = request.Description;
        vmss.ResourceGroup = request.ResourceGroup;

        vmss.Environments.Clear();
        foreach (var environment in environments ?? new List<Environment>())
        {
            vmss.Environments.Add(environment);
        }

        var entity = _context.VirtualMachines.Update(vmss);

        await _context.SaveChangesAsync(ct);
        return entity.Entity.Id;
    }
}

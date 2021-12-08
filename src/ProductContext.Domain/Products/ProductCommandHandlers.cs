using System.Threading;
using System.Threading.Tasks;

using AggregateSource.EventStore;
using AggregateSource.EventStore.Snapshots;

using MediatR;

using ProductContext.Domain.Contracts;
using ProductContext.Framework;

namespace ProductContext.Domain.Products
{
	public class AddContentToProductCommandHandler : AsyncRequestHandler<Commands.V1.AddContentToProduct>
	{
		private readonly Now _now;
		private readonly AsyncSnapshotableRepository<Product> _snapshotableRepository;

		public AddContentToProductCommandHandler(AsyncSnapshotableRepository<Product> snapshotableRepository, Now now)
		{
			_snapshotableRepository = snapshotableRepository;
			_now = now;
		}

		protected override async Task Handle(Commands.V1.AddContentToProduct request, CancellationToken cancellationToken) =>
			await _snapshotableRepository.UpdateWhen(request.ProductId, _now, async product =>
			{
				product.AddContent(request.ProductContentId, request.Description, request.VariantTypeValueId);
				await Task.CompletedTask;
			});
	}

	public class AddVariantToProductCommandHandler : AsyncRequestHandler<Commands.V1.AddVariantToProduct>
	{
		private readonly Now _now;
		private readonly AsyncSnapshotableRepository<Product> _snapshotableRepository;

		public AddVariantToProductCommandHandler(Now now, AsyncSnapshotableRepository<Product> snapshotableRepository)
		{
			_now = now;
			_snapshotableRepository = snapshotableRepository;
		}

		protected override async Task Handle(Commands.V1.AddVariantToProduct request, CancellationToken cancellationToken) =>
			await _snapshotableRepository.UpdateWhen(request.ProductId, _now, async product =>
			{
				product.AddVariant(request.ContentId, request.VariantId, request.Barcode, request.VariantTypeValueId);
				await Task.CompletedTask;
			});
	}

	public class ProductCommandHandlers : AsyncRequestHandler<Commands.V1.CreateProduct>
	{
		private readonly Now _now;
		private readonly AsyncRepository<Product> _repository;

		public ProductCommandHandlers(Now now, AsyncRepository<Product> repository)
		{
			_now = now;
			_repository = repository;
		}

		protected override async Task Handle(Commands.V1.CreateProduct command, CancellationToken cancellationToken)
		{
			Product product = Product.Create(
				command.ProductId,
				command.BrandId,
				command.Code,
				command.BusinessUnitId);

			await _repository.AddWhen(product, _now, async (p, repository) =>
			{
				repository.Add(p.ProductId, p);
				await Task.CompletedTask;
			});
		}
	}
}

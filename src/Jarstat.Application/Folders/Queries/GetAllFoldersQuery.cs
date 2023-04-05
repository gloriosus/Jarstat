﻿using Jarstat.Domain.Entities;
using Jarstat.Domain.Shared;
using MediatR;

namespace Jarstat.Application.Queries;

public class GetAllFoldersQuery : IRequest<Result<Assortment<Folder>>> { }

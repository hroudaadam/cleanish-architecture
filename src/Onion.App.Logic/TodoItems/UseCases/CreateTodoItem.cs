﻿using FluentValidation;
using MediatR;
using Onion.App.Data.Database.Entities;
using Onion.App.Data.Database.Repositories;
using Onion.App.Logic.Common.Attributes;
using Onion.App.Logic.Common.Security;
using Onion.App.Logic.TodoLists.Models;
using Onion.Shared.Mapper;
using System.Threading;

namespace Onion.App.Logic.TodoItems.UseCases;

[Authorize]
public class CreateTodoItemRequest : IRequest<TodoItemRes>
{
    public string Title { get; set; }
    public Guid TodoListId { get; set; }
}

internal class CreateTodoItemRequestValidator : AbstractValidator<CreateTodoItemRequest>
{
    public CreateTodoItemRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.TodoListId).NotEmpty();
    }
}

internal class CreateTodoItemRequestHandler : IRequestHandler<CreateTodoItemRequest, TodoItemRes>
{
    private readonly ISecurityContextProvider _securityContextProvider;
    private readonly IObjectMapper _mapper;
    private readonly ITodoItemRepository _todoItemRepository;
    private readonly ITodoListRepository _todoListRepository;

    public CreateTodoItemRequestHandler(
        ISecurityContextProvider securityContextProvider,
        IObjectMapper mapper,
        ITodoItemRepository todoItemRepository,
        ITodoListRepository todoListRepository)
    {
        _securityContextProvider = securityContextProvider;
        _mapper = mapper;
        _todoItemRepository = todoItemRepository;
        _todoListRepository = todoListRepository;
    }

    public async Task<TodoItemRes> Handle(CreateTodoItemRequest request, CancellationToken cancellationToken)
    {
        var subjectId = _securityContextProvider.GetSubjectId();
        await TodoItemCommonLogic.ValidateTodoListOwnership(_todoListRepository, request.TodoListId, subjectId);

        var newTodoItem = _mapper.Map<TodoItem>(request);
        newTodoItem = await _todoItemRepository.CreateAsync(newTodoItem);
        return _mapper.Map<TodoItemRes>(newTodoItem);
    }
}

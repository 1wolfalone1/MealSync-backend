using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;
using Microsoft.AspNetCore.Http;

namespace MealSync.Application.UseCases.Storages.Commands.DeleteFile;

public class DeleteFileCommand: ICommand<Result>
{
    public string Url { get; set; }
}
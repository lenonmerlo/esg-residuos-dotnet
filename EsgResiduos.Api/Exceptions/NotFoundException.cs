namespace EsgResiduos.Api.Exceptions;

public class NotFoundException(string resource, int id) : AppException($"{resource} com id {id} não encontrado.", 404)
{
}

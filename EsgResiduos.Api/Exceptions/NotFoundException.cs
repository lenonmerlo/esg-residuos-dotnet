namespace EsgResiduos.Api.Exceptions;

public class NotFoundException(string resource, int id) : AppException($"{resource} ArrayWithOffset id {id} not found.", 404)
{
}
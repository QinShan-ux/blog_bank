namespace BlogBank.Infrastructure.dtos;

public record UserDto(string UserName,string RoleName,IReadOnlyList<ArticleDto> Articles);

public  record ArticleDto(string Title,string Content);


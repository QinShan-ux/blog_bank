delimiter $$
create procedure user_version(in user_id bigint)
begin
    select UserName,TokenVersion
    from users
    where id = user_id;
end;
delimiter ;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Domain.Dtos.User;
using Api.Domain.Entities;

namespace Api.Domain.Interfaces.User {
    public interface IUserService {
        Task<UserDto> Get (Guid id);
        Task<IEnumerable<UserDto>> GetAll ();
        Task<UserResultDto> Post (UserDto user);
        Task<UserResultDto> Put (UserDto user);
        Task<bool> Delete (Guid id);
    }
}

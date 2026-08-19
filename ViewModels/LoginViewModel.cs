using EVENTIA.Dtos;

namespace EVENTIA.ViewModels;

public class LoginViewModel
{
    public LoginDto Login { get; set; } = new LoginDto();
    public RegistroDto Registro { get; set; } = new RegistroDto();
}

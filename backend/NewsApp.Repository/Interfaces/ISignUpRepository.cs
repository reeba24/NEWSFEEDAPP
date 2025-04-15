using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using newsapp.Models;

namespace NewsApp.Repository.Interfaces
{
    public interface ISignUpRepository
    {
        string CreateSignUp(Signup signup);

    }
}

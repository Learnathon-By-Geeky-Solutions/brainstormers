﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskForge.Application.Interfaces.Services
{
	public interface ILinkGeneratorService
	{
		string GenerateInvitationLink(string returnUrl);
	}

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IdentityModel.Services;
using System.Security.Claims;
using System.Data;

public partial class wif : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Request.HttpMethod == "GET" && !User.Identity.IsAuthenticated)
            FederatedAuthentication.WSFederationAuthenticationModule.SignIn("LoginPageSignInSubmit");
        else
        {
            string sql;
            ClaimsPrincipal claimsPrincipal = User as ClaimsPrincipal;

            Claim claimWindowsAccount = claimsPrincipal.FindFirst(ClaimTypes.WindowsAccountName);
			Claim claimEmail = claimsPrincipal.FindFirst(ClaimTypes.Email);
            // Fetch the user's information from the users table
            sql = @"select us_id, us_username
			from users
			where (us_username = N'$us' OR us_email = N'$ue')
			and us_active = 1";
            sql = sql.Replace("$us", claimWindowsAccount.Value.Replace("'", "''"));
			if(!String.IsNullOrEmpty(claimEmail.Value))
				sql = sql.Replace("$ue", claimEmail.Value.Replace("'", "''"));

            DataRow dr = btnet.DbUtil.get_datarow(sql);
            if (dr != null)
            {
                // The user was found, so bake a cookie and redirect
                int userid = (int)dr["us_id"];
                btnet.Security.create_session(
                    Request,
                    Response,
                    userid,
                    (string)dr["us_username"],
                    "1");

                btnet.Util.update_most_recent_login_datetime(userid);

                btnet.Util.redirect(Request, Response);
            }
            else
            {
                // lets try auto registering the user...
                
                Claim claimSurname = claimsPrincipal.FindFirst(ClaimTypes.Surname);
                Claim claimGivenName = claimsPrincipal.FindFirst(ClaimTypes.GivenName);
                int new_user_id = btnet.User.copy_user(
                claimWindowsAccount.Value,
                claimEmail.Value,
                claimSurname.Value,
                claimSurname.Value,
                String.Empty,
                0, // salt
                Guid.NewGuid().ToString(), // random value for password
                "guest",
                false);

                if (new_user_id > 0) // automatically created the user
                {
                    // The user was created, so bake a cookie and redirect
                    btnet.Security.create_session(
                        Request,
                        Response,
                        new_user_id,
                        claimWindowsAccount.Value.Replace("'", "''"),
                        "1");

                    btnet.Util.update_most_recent_login_datetime(new_user_id);

                    btnet.Util.redirect(Request, Response);
                }
            }


        }
    }
}
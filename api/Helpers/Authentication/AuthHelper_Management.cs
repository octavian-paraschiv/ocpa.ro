using Microsoft.AspNetCore.Http;
using ocpa.ro.api.Exceptions;
using ocpa.ro.api.Models.Applications;
using ocpa.ro.api.Models.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ocpa.ro.api.Helpers.Authentication
{
    public interface IAuthHelperManagement
    {
        //------------------------

        IEnumerable<Application> GetApplications();
        Application SaveApplication(Application app, out bool inserted);
        int DeleteApplication(int appId);

        //------------------------

        IEnumerable<Menu> GetMenus();
        Menu SaveMenu(Menu menu, out bool inserted);
        int DeleteMenu(int menuId);

        //------------------------

        IEnumerable<ApplicationMenu> GetApplicationMenus();
        ApplicationMenu SaveApplicationMenu(int appId, int menuId, out bool inserted);
        int DeleteApplicationMenu(int appId, int menuId);

        //------------------------

        IEnumerable<ApplicationUser> GetApplicationUsers(int appId);
        ApplicationUser SaveApplicationUser(int appId, ApplicationUser appUser, out bool inserted);
        int DeleteApplicationUser(int appId, int appUserId);
    }

    public partial class AuthHelper : IAuthHelper
    {
        //------------------------

        public IEnumerable<Application> GetApplications()
        {
            try
            {
                return _db.Table<Application>();
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return Array.Empty<Application>();
        }

        public Application SaveApplication(Application app, out bool inserted)
        {
            Application dbu = null;
            inserted = false;

            try
            {
                var id = app.Id;

                dbu = _db.Table<Application>().FirstOrDefault(a => id == a.Id);
                if (dbu?.Builtin ?? false)
                    throw new ExtendedException("ERR_EDIT_BUILT_IN_APP");

                bool newEntry = (dbu == null);

                dbu ??= new Application();

                dbu.LoginRequired = app.LoginRequired;
                dbu.Code = app.Code;
                dbu.Name = app.Name;
                dbu.AdminMode = app.AdminMode;

                if (newEntry)
                {
                    dbu.Builtin = false; // Can't create a new bultin app
                    dbu.Id = (_db.Table<Application>().OrderByDescending(u => u.Id).FirstOrDefault()?.Id ?? 0) + 1;

                    if (_db.Insert(dbu) > 0)
                        inserted = true;
                    else
                        dbu = null;
                }
                else
                {
                    if (_db.Update(dbu) <= 0)
                        dbu = null;
                }
            }
            catch (ExtendedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                LogException(ex);
                dbu = null;
            }

            return dbu;
        }

        public int DeleteApplication(int appId)
        {
            try
            {
                var dbu = _db.Table<Application>().FirstOrDefault(u => u.Id == appId);
                if (dbu == null)
                    return StatusCodes.Status404NotFound;

                if (dbu.Builtin)
                    throw new ExtendedException($"ERR_DELETE_BUILTIN_APP");

                if (_db.Table<ApplicationMenu>().Any(am => am.ApplicationId == appId))
                    throw new ExtendedException($"ERR_DELETE_APP_MENU_ASSOCIATIONS");

                if (_db.Table<ApplicationUser>().Any(am => am.ApplicationId == appId))
                    throw new ExtendedException($"ERR_DELETE_APP_USER_ASSOCIATIONS");

                if (_db.Delete(dbu) > 0)
                    return StatusCodes.Status200OK;
            }
            catch (ExtendedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return StatusCodes.Status400BadRequest;
        }

        //------------------------

        public IEnumerable<Menu> GetMenus()
        {
            try
            {
                return _db.Table<Menu>();
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return Array.Empty<Menu>();
        }

        public Menu SaveMenu(Menu menu, out bool inserted)
        {
            Menu dbu = null;
            inserted = false;

            try
            {
                var id = menu.Id;

                dbu = _db.Table<Menu>().FirstOrDefault(a => id == a.Id);
                if (dbu?.Builtin ?? false)
                    throw new ExtendedException($"ERR_EDIT_BUILTIN_MENU");

                bool newEntry = (dbu == null);

                dbu ??= new Menu();

                dbu.DisplayModeId = menu.DisplayModeId;
                dbu.MenuIcon = menu.MenuIcon;
                dbu.Name = menu.Name;
                dbu.Url = menu.Url;

                if (newEntry)
                {
                    dbu.Builtin = false; // can't create a new builtin menu
                    dbu.Id = (_db.Table<Menu>().OrderByDescending(u => u.Id).FirstOrDefault()?.Id ?? 0) + 1;

                    if (_db.Insert(dbu) > 0)
                        inserted = true;
                    else
                        dbu = null;
                }
                else
                {
                    if (_db.Update(dbu) <= 0)
                        dbu = null;
                }
            }
            catch (ExtendedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                LogException(ex);
                dbu = null;
            }

            return dbu;
        }

        public int DeleteMenu(int menuId)
        {
            try
            {
                var dbu = _db.Table<Menu>().FirstOrDefault(u => u.Id == menuId);
                if (dbu == null)
                    return StatusCodes.Status404NotFound;

                if (dbu.Builtin)
                    throw new ExtendedException($"ERR_DELETE_BUILTIN_MENU");

                if (_db.Table<ApplicationMenu>().Any(am => am.MenuId == menuId))
                    throw new ExtendedException($"ERR_DELETE_MENU_APP_ASSOCIATIONS");

                if (_db.Delete(dbu) > 0)
                    return StatusCodes.Status200OK;
            }
            catch (ExtendedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return StatusCodes.Status400BadRequest;
        }

        //------------------------

        public IEnumerable<ApplicationMenu> GetApplicationMenus()
        {
            try
            {
                return _db.Table<ApplicationMenu>();
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return Array.Empty<ApplicationMenu>();
        }

        public ApplicationMenu SaveApplicationMenu(int appId, int menuId, out bool inserted)
        {
            inserted = false;

            ValidateAppId(appId);
            ValidateMenuId(menuId);

            try
            {
                var dbu = _db.Table<ApplicationMenu>().FirstOrDefault(a => a.ApplicationId == appId && a.MenuId == menuId);
                if (dbu != null)
                    return dbu; // Already present

                dbu = new ApplicationMenu
                {
                    ApplicationId = appId,
                    MenuId = menuId,
                    Id = (_db.Table<ApplicationMenu>().OrderByDescending(u => u.Id).FirstOrDefault()?.Id ?? 0) + 1
                };

                if (_db.Insert(dbu) > 0)
                {
                    inserted = true;
                    return dbu;
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return null;
        }

        public int DeleteApplicationMenu(int appId, int menuId)
        {
            ValidateAppId(appId);
            ValidateMenuId(menuId);

            try
            {
                var dbu = _db.Table<ApplicationMenu>().FirstOrDefault(a => a.ApplicationId == appId && a.MenuId == menuId);
                if (dbu == null)
                    return StatusCodes.Status404NotFound;

                if (_db.Delete(dbu) > 0)
                    return StatusCodes.Status200OK;
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return StatusCodes.Status400BadRequest;
        }

        //------------------------

        public IEnumerable<ApplicationUser> GetApplicationUsers(int appId)
        {
            ValidateAppId(appId);

            try
            {
                return _db.Table<ApplicationUser>().Where(au => au.ApplicationId == appId);
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return Array.Empty<ApplicationUser>();
        }

        public ApplicationUser SaveApplicationUser(int appId, ApplicationUser appUser, out bool inserted)
        {

            ApplicationUser dbu = null;
            inserted = false;

            ValidateAppId(appId);

            try
            {
                var id = appUser.Id;

                dbu = _db.Table<ApplicationUser>().FirstOrDefault(a => id == a.Id && appId == a.ApplicationId);

                bool newEntry = (dbu == null);

                dbu ??= new ApplicationUser();

                dbu.ApplicationId = appId;
                dbu.UserId = appUser.UserId;

                if (newEntry)
                {
                    dbu.Id = (_db.Table<ApplicationUser>().OrderByDescending(u => u.Id).FirstOrDefault()?.Id ?? 0) + 1;

                    if (_db.Insert(dbu) > 0)
                        inserted = true;
                    else
                        dbu = null;
                }
                else
                {
                    if (_db.Update(dbu) <= 0)
                        dbu = null;
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
                dbu = null;
            }

            return dbu;
        }

        public int DeleteApplicationUser(int appId, int appUserId)
        {
            ValidateAppId(appId);

            try
            {
                var dbu = _db.Table<ApplicationUser>().FirstOrDefault(u => u.Id == appUserId && u.ApplicationId == appId);
                if (dbu == null)
                    return StatusCodes.Status404NotFound;

                if (_db.Delete(dbu) > 0)
                    return StatusCodes.Status200OK;
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return StatusCodes.Status400BadRequest;
        }

        //------------------------

        private void ValidateAppId(int appId)
        {
            try
            {
                var app = _db.Table<Application>().FirstOrDefault(a => a.Id == appId);
                if (app == null)
                    throw new ExtendedException("ERR_APP_NOT_FOUND");
            }
            catch (Exception ex)
            {
                LogException(ex);
                throw;
            }
        }

        private void ValidateMenuId(int menuId)
        {
            try
            {
                var app = _db.Table<Menu>().FirstOrDefault(a => a.Id == menuId);
                if (app == null)
                    throw new ExtendedException("ERR_MENU_NOT_FOUND");
            }
            catch (Exception ex)
            {
                LogException(ex);
                throw;
            }
        }
    }
}

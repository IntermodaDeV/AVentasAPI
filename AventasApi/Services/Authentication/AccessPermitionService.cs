using AventasApi.Infrastructure;
using AventasApi.Models.Authentication;
using System.Collections.Generic;
using System.Linq;

namespace AventasApi.Services.Authentication
{
    public class AccessPermitionService
    {
        public List<Menu> GetAccessPermission(List<PantallasxUsuario> accessUser, List<Pantallas> pantallas, string type)
        {
            List<Menu> menu = new List<Menu>();

            pantallas.RemoveAll(x => x.Activa == false);

            switch (type)
            {
                case "plain":
                    menu = (from m in accessUser.Select(x => new { MenuId = x.IdPantalla, Activa = x.Activa, By = "Users" })
                            join f in pantallas on m.MenuId equals f.IdPantalla
                            select new Menu
                            {
                                IdMenu = m.MenuId??0,
                                Nombre = f.Nombre,
                                Icono = f.Icono,
                                Ruta = f.Ruta,
                                Por = m.By
                            }
                            ).ToList();
                    break;
                case "tree":
                    CreateMenu(ref menu, pantallas, accessUser, "User");
                    break;
            }

            return menu;
        }

        private void CreateMenu(ref List<Menu> menu, List<Pantallas> pantallas, List<PantallasxUsuario> access, string by)
        {
            if (access.Count == 0)
                return;

            foreach (var item in access)
            {
                var form = pantallas.FirstOrDefault(x => x.IdPantalla == item.IdPantalla);
                var isFatherMenu = form.IdPantallaPadre == null;

                if (Exist(menu, item.IdPantalla) == false)
                    menu.Add(new Menu { IdMenu = form.IdPantalla, Nombre = form.Nombre, Icono = form.Icono, Ruta = form.Ruta, MenuHijos = new List<Menu>(), Por = by });

                if (isFatherMenu == true)
                {
                    var childrenMenuIds = pantallas.Where(x => x.IdPantallaPadre == form.IdPantalla).Select(x => x.IdPantalla);
                    var childrenMenus = access.Where(x => childrenMenuIds.Contains(x.IdPantalla)).ToList();
                    var selectMenu = menu.FirstOrDefault(x => x.IdMenu == form.IdPantalla).MenuHijos;

                    CreateMenu(ref selectMenu, pantallas, childrenMenus, by);
                }
            }
        }

        private bool Exist(List<Menu> menu, int formId)
        {
            if (menu.Count == 0)
                return false;
            else
            {
                if (menu.FirstOrDefault(x => x.IdMenu == formId) == null)
                {
                    bool exist = false;
                    foreach (var item in menu)
                    {
                        exist = Exist(item.MenuHijos, formId);
                        if (exist == true)
                            break;
                    }
                    return exist;
                }
                else
                {
                    return true;
                }
            }
        }
    }
}
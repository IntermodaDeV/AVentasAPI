using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using AventasApi.ImageManager;
using DBData.Database;
//using AventasApi.Models.ApiModels;
using AventasApi.Models.ViewModels;

namespace AventasApi.Services.AsyncJobs
{
    public static class AsyncSqlInsert
    {
        public static TaskFactory factory;
        static AsyncSqlInsert()
        {
            factory = new TaskFactory();
        }
        public static bool IngresarPedido(PedidosxCliente pedido, string firma,string empresa)
        {
            try
            {
                bool value = true;
                using (AVentasEntities context = new AVentasEntities())
                {
                    context.PedidosxCliente.Add(pedido);
                    int rowAffected = context.SaveChanges();
                    if (rowAffected > 0)
                    {
                        ValidarCorrelativoPedido(pedido.CodigoAsesor,empresa);
                    }
                    else
                    {
                        value = false;
                    }
                    ByteArrayImageConversion firmaConversion = new ByteArrayImageConversion(firma);
                    if (firmaConversion.IsSuccesful)
                    {

                        FirmasxPedido firmaAGuardar = new FirmasxPedido
                        {
                            PedidoId = pedido.PedidoId,
                            Firma = firmaConversion.ContentByteArray
                        };
                        context.FirmasxPedido.Add(firmaAGuardar);
                        context.SaveChanges();
                    }
                    value = true;
                }

                return value;
            }
            catch(Exception e)
            {
                return false;
            }
        }

        public static bool IngresarDevolucion(Devolucion devolucion, string empresa)
        {
            try
            {
                bool value = true;
                using (AVentasEntities context = new AVentasEntities())
                {
                    context.Devolucion.Add(devolucion);
                    int rowAffected = context.SaveChanges();
                    if (rowAffected > 0)
                    {
                        ValidarCorrelativoDevolucion(devolucion.CodigoAsesor, empresa);
                    }
                    else
                    {
                        value = false;
                    }
                    
                    value = true;
                }

                return value;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public static bool IngresarInventaio(InventariosCliente inventario, string asesor, string empresa)
        {
            try
            {
                bool value = true;
                using (AVentasEntities context = new AVentasEntities())
                {
                    context.InventariosCliente.Add(inventario);
                    int rowAffected = context.SaveChanges();
                    if (rowAffected > 0)
                    {
                        ValidarCorrelativoInventario(asesor, empresa);
                    }
                    else
                    {
                        value = false;
                    }
                    
                    value = true;
                }

                return value;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private static void ValidarCorrelativoPedido(string CodigoAsesor,string empresa)
        {
            using(AVentasEntities context = new AVentasEntities())
            {
                try
                {
                    var asesor = context.Asesores.FirstOrDefault(x => x.CodigoAsesor == CodigoAsesor && x.EmpresaId == empresa);

                    var correlativo = $"{asesor.InicialesNombre}-{100000+ (asesor.CorrelativoPedidos != null ? asesor.CorrelativoPedidos : 0)}";

                    if(context.PedidosxCliente.FirstOrDefault(x => x.PedidoId == correlativo) == null)
                    {
                        return;
                    }
                    else
                    {
                        asesor.CorrelativoPedidos = (asesor.CorrelativoPedidos != null ? asesor.CorrelativoPedidos : 0) + 1;
                        context.SaveChanges();
                        ValidarCorrelativoPedido(CodigoAsesor,empresa);
                    }

                }
                catch (Exception ex)
                {

                }
            }
        }

        private static void ValidarCorrelativoDevolucion(string CodigoAsesor, string empresa)
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                try
                {
                    var asesor = context.Asesores.FirstOrDefault(x => x.CodigoAsesor == CodigoAsesor && x.EmpresaId == empresa);
                    asesor.CorrelativoDevolucion = (asesor.CorrelativoDevolucion != null ? asesor.CorrelativoDevolucion : 0) + 1;
                    context.SaveChanges();

                    var correlativo = $"{asesor.InicialesNombre}-{100000 + (asesor.CorrelativoDevolucion != null ? asesor.CorrelativoDevolucion : 0)}";

                    if (context.Devolucion.FirstOrDefault(x => x.NumDevolucion == correlativo) == null)
                    {
                        return;
                    }
                    else
                    {
                        ValidarCorrelativoDevolucion(CodigoAsesor, empresa);
                    }

                }
                catch (Exception ex)
                {

                }
            }
        }
        
        public static void ValidarCorrelativoInventario(string CodigoAsesor, string empresa)
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                try
                {
                    var asesor = context.Asesores.FirstOrDefault(x => x.CodigoAsesor == CodigoAsesor && x.EmpresaId == empresa);
                    asesor.CorrelativoInventario = (asesor.CorrelativoInventario != null ? asesor.CorrelativoInventario : 0) + 1;
                    context.SaveChanges();

                    var correlativo = $"{asesor.InicialesNombre}-{100000 + (asesor.CorrelativoInventario != null ? asesor.CorrelativoInventario : 0)}";

                    if (context.Devolucion.FirstOrDefault(x => x.NumDevolucion == correlativo) == null)
                    {
                        return;
                    }
                    else
                    {
                        ValidarCorrelativoInventario(CodigoAsesor, empresa);
                    }

                }
                catch (Exception ex)
                {

                }
            }
        }
        
        public static void ValidarCorrelativoRecibo(string CodigoAsesor, string empresa)
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                try
                {
                    var asesor = context.Asesores.FirstOrDefault(x => x.CodigoAsesor == CodigoAsesor && x.EmpresaId == empresa);
                    var correlativo = $"{asesor.InicialesNombre}-{100000 + (asesor.CorrelativoRecibos != null ? asesor.CorrelativoRecibos : 0)}";

                    if (context.RecibosxCliente.FirstOrDefault(x => x.NumeroRecibo == correlativo) == null && context.AnticiposxCliente.FirstOrDefault(x => x.NumeroRecibo == correlativo) == null)
                    {
                        return;
                    }
                    else
                    {
                        asesor.CorrelativoRecibos = (asesor.CorrelativoRecibos != null ? asesor.CorrelativoRecibos : 0) + 1;
                        context.SaveChanges();
                        ValidarCorrelativoRecibo(CodigoAsesor, empresa);
                    }

                }
                catch (Exception ex)
                {

                }
            }
        }

        public static void IngresarPedidoFlotante(PedidosxClienteFlotante pedido, string firma)
        {
            var pedidoTask = Task.Run(() =>
            {
                using (AVentasEntities context = new AVentasEntities())
                {
                    context.PedidosxClienteFlotante.Add(pedido);
                    context.SaveChanges();
                    ByteArrayImageConversion firmaConversion = new ByteArrayImageConversion(firma);
                    if (firmaConversion.IsSuccesful)
                    {

                        FirmasxPedido firmaAGuardar = new FirmasxPedido
                        {
                            PedidoId = pedido.PedidoId,
                            Firma = firmaConversion.ContentByteArray
                        };
                        context.FirmasxPedido.Add(firmaAGuardar);
                        context.SaveChanges();
                    }
                }

            });
        }

        public static bool IngresarRecibos(List<RecibosxClienteViewModel> recibos,bool sincronizado)
        {
            try
            {

                    var reciboAAgregar = recibos.Select(rec => new RecibosxCliente
                    {
                        NumeroRecibo = rec.NumeroRecibo,
                        CodigoCliente = rec.CodigoCliente,
                        Fecha = rec.Fecha,
                        IdTipoPago = rec.IdTipoPago,
                        Referencia = rec.Referencia,
                        FechaCheque = rec.FechaPago,
                        IdBanco = rec.IdBanco,
                        Valor = rec.Valor,
                        IdMoneda = rec.IdMoneda,
                        Sincronizado = sincronizado,
                        CodigoAsesor = rec.CodigoAsesor,
                        IdFactura = rec.IdFactura,
                        Descuento = rec.Descuento,
                        Longitude = rec.Longitude,
                        Latitude = rec.Latitude,
                        SpecPago = rec.SpecPago,
                        UsuarioCreacion = rec.UsuarioCreacion,
                        FechaCreacion = rec.FechaCreacion,
                        proformaId = rec.proformaId,
                        Origen = "Web",
                        //firma = rec.firmaByte,
                        Anulado=false,
                        Reimpresion = false,
                        RecibosDetalle = rec.DetalleRecibo.Select(recDet => new RecibosDetalle
                        {
                            IdReciboDetalle = recDet.IdReciboDetalle,
                            ReciboId = recDet.ReciboId,
                            IdSubFactura = recDet.IdSubFactura,
                            Valor = recDet.Valor,
                            Descuento = recDet.Descuento,
                            EsAbono = recDet.EsAbono,
                            ValorFactura = recDet.ValorFactura
                        }).ToList()
                    }).ToList();

                    using (AVentasEntities context = new AVentasEntities())
                    {
                        context.RecibosxCliente.AddRange(reciboAAgregar);
                        int affectedRows = context.SaveChanges();

                    if (affectedRows > 0)
                    {
                        ValidarCorrelativoRecibo(recibos[0].CodigoAsesor, recibos[0].EmpresaUsuario);
                    }

                        return false;
                    }
            }
            catch
            {
                return true;
            }
        }

        public static bool IngresarRecibosProforma(List<RecibosxClienteViewModel> recibos)
        {
            try
            {

                var reciboAAgregar = recibos.Select(rec => new RecibosProforma
                {
                    NumeroProforma = rec.NumeroRecibo,
                    CodigoCliente = rec.CodigoCliente,
                    Fecha = rec.Fecha,
                    IdTipoPago = rec.IdTipoPago,
                    Referencia = rec.Referencia,
                    FechaCheque = rec.FechaPago,
                    IdBanco = rec.IdBanco,
                    Valor = rec.Valor,
                    IdMoneda = rec.IdMoneda,
                    CodigoAsesor = rec.CodigoAsesor,
                    IdFactura = rec.IdFactura,
                    Descuento = rec.Descuento,
                    SpecPago = rec.SpecPago,
                    UsuarioCreacion = rec.UsuarioCreacion,
                    FechaCreacion = rec.FechaCreacion,
                    RecibosProformaDetalle = rec.DetalleRecibo.Select(recDet => new RecibosProformaDetalle
                    {
                        IdProformaDetalle = recDet.IdReciboDetalle,
                        ProformaId = recDet.ReciboId,
                        IdSubFactura = recDet.IdSubFactura,
                        Valor = recDet.Valor,
                        Descuento = recDet.Descuento,
                        EsAbono = recDet.EsAbono.Value,
                    }).ToList()
                }).ToList();

                using (AVentasEntities context = new AVentasEntities())
                {
                    context.RecibosProforma.AddRange(reciboAAgregar);
                    context.SaveChanges();
                    return false;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                return true;
            }
        }

        public static void IngresarRecibosFlotante(List<RecibosxClienteFlotanteViewModel> recibos)
        {
            var reciboTask = Task.Run(() =>
            {

                var reciboAAgregar = recibos.Select(rec => new RecibosxClienteFlotante
                {
                    NumeroRecibo = rec.NumeroRecibo,
                    CodigoCliente = rec.CodigoCliente,
                    Fecha = rec.Fecha,
                    IdTipoPago = rec.IdTipoPago,
                    Referencia = rec.Referencia,
                    FechaCheque = rec.FechaPago,
                    IdBanco = rec.IdBanco,
                    Valor = rec.Valor,
                    IdMoneda = rec.IdMoneda,
                    Sincronizado = false,
                    CodigoAsesor = rec.CodigoAsesor,
                    IdFactura = rec.IdFactura,
                    Descuento = rec.Descuento,
                    Longitude = rec.Longitude,
                    Latitude = rec.Latitude,
                    SpecPago = rec.SpecPago,
                    UsuarioCreacion = rec.UsuarioCreacion,
                    FechaCreacion = rec.FechaCreacion,
                    RecibosDetalleFlotante = rec.DetalleRecibo.Select(recDet => new RecibosDetalleFlotante
                    {
                        IdReciboDetalle = recDet.IdReciboDetalle,
                        ReciboId = recDet.ReciboId,
                        IdSubFactura = recDet.IdSubFactura,
                        Valor = recDet.Valor,
                        Descuento = recDet.Descuento,
                        EsAbono = recDet.EsAbono,
                    }).ToList()
                }).ToList();

                using (AVentasEntities context = new AVentasEntities())
                {
                    context.RecibosxClienteFlotante.AddRange(reciboAAgregar);
                    context.SaveChanges();
                }

            });
        }
    }
}
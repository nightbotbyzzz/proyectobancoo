using BancoSimple2M5.Models;
using BancoSimple2M5.Data;
using Microsoft.EntityFrameworkCore;

namespace BancoSimple2M5.Services
{
    public class CuentaService
    {
        private readonly BancoSimple2M5Context _context;

        public CuentaService(BancoSimple2M5Context context)
        {
            _context = context;
        }

        public bool CrearCuenta(string numeroCuenta, decimal saldo, int clienteId, out Cuenta nuevaCuenta)
        {
            nuevaCuenta = null;
            if (string.IsNullOrWhiteSpace(numeroCuenta)) return false;

            nuevaCuenta = new Cuenta
            {
                NumeroCuenta = numeroCuenta,
                Saldo = saldo,
                ClienteId = clienteId,
                Activo = true
            };

            _context.Cuentas.Add(nuevaCuenta);
            _context.SaveChanges();
            return true;
        }

        public bool Transferir(int origenId, int destinoId, decimal monto, out string error)
        {
            error = null;

            using var transaccion = _context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
            try
            {
                var cuentaOrigen = _context.Cuentas.FirstOrDefault(c => c.CuentaId == origenId);
                var cuentaDestino = _context.Cuentas.FirstOrDefault(c => c.CuentaId == destinoId);

                if (cuentaOrigen == null || cuentaDestino == null)
                {
                    error = "Cuenta no encontrada.";
                    return false;
                }

                if (cuentaOrigen.Saldo < monto)
                {
                    error = "Saldo insuficiente.";
                    return false;
                }

                cuentaOrigen.Saldo -= monto;
                cuentaDestino.Saldo += monto;

                _context.Transacciones.Add(new Transaccion
                {
                    Monto = monto,
                    Fecha = DateTime.Now,
                    Descripcion = "Transferencia",
                    CuentaOrigenId = origenId,
                    CuentaDestinoId = destinoId
                });

                _context.SaveChanges();
                transaccion.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaccion.Rollback();
                error = ex.Message;
                return false;
            }
        }
    }
}
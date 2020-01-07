using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;

namespace Compilador_CSharp
{

    public class Nodo
    {
        #region Nodos
        public Nodo hijoIzquierdo;
        public Nodo hijoCentro;
        public Nodo hijoDerecho;
        public Nodo Hermano;
        #endregion

        #region Propiedades de Nodo
        public string lexema;
        public string operador;

        public tipoExpresion miTipoExpresion;
        public TipoDato miTipoValor;
        public tipoSentencia miTipoSentencia;
        public tipoOperacion miTipoOperacion;
            
        #endregion
        
        #region Comprobacion de Tipos
        public TipoDato tipoValorHijoIzquierdo;
        public TipoDato tipoValorHijoDerecho;
        #endregion
    }

    public class ArbolSintactico
    {
        #region Propiedades del Arbol
        int puntero = 0;
        public string codigo_p = "";
        
        ListaToken TokenActual;
        List<ListaToken> miListaTokens;
        TablaSimbolos miTablaSimbolos;
        #endregion
        
        public static Nodo Arbol = new Nodo();

        #region Metodos para Generar y Recorrer el Arbol
        public Nodo GenerarArbol(List<ListaToken> lista, TablaSimbolos tabla)
        {
            miListaTokens = lista;
            miTablaSimbolos = tabla;
            Arbol = GenerarSentencias();
            return Arbol;
        }
        
        public void ObtenerSiguienteToken()
        {
            puntero++;
            if (puntero < miListaTokens.Count)
            {
                TokenActual = miListaTokens[puntero];
            }

        }
        
        public Nodo GenerarSentencias()
        {
            Nodo t = new Nodo();
            if (puntero < miListaTokens.Count)
            {
                if (miListaTokens[puntero].Token == 100 && miListaTokens[puntero + 1].Lexema == "=")
                {
                    codigo_p += "lda " + miListaTokens[puntero].Lexema + "\r\n";
                    t = Asignacion();

                }
                else
                {
                    ObtenerSiguienteToken();
                    t = GenerarSentencias();
                }

            }
            GenerarCodigoP(codigo_p);
            return t;
        }
        
        public void GenerarCodigoP(string codigop)
        {
            string ubicacion = @"C:\Users\Marti\OneDrive\Escritorio\CodigoP.txt";
            File.WriteAllText(ubicacion, codigop);
        }

        public void RecorridoPostOrden(Nodo miArbol)
        {
            if (miArbol.hijoIzquierdo != null)
            {
                RecorridoPostOrden(miArbol.hijoIzquierdo);
            }

            if (miArbol.hijoCentro != null)
            {
                RecorridoPostOrden(miArbol.hijoCentro);
            }

            if (miArbol.hijoDerecho != null)
            {
                RecorridoPostOrden(miArbol.hijoDerecho);
            }

            if (miArbol.lexema != null)
            {
                MessageBox.Show(miArbol.lexema);
                if (miArbol.Hermano != null)
                {
                    RecorridoPostOrden(miArbol.Hermano);
                }
            }
        }
        #endregion

        #region Metodos de Asignacion
        public Nodo Asignacion()
        {
            Nodo t = NuevoNodoSentencia();
            t.lexema = TokenActual.Lexema;
            t.miTipoSentencia = tipoSentencia.Asignacion;
            t.operador = miListaTokens[puntero + 1].Lexema;
            ObtenerSiguienteToken();
            ObtenerSiguienteToken();
            t.hijoIzquierdo = SimpleExpresion();
            codigo_p += "sto" + "\r\n";
            t.Hermano = GenerarSentencias();
            return t;
        }

        public Nodo SimpleExpresion()
        {
            Nodo t = Termino();
            while (miListaTokens[puntero].Lexema == "+" || miListaTokens[puntero].Lexema == "-")
            {
                Nodo p = NuevoNodoExpresion();
                string lexema_anterior = "";

                p.lexema = TokenActual.Lexema;
                p.miTipoExpresion = tipoExpresion.Aritmetica;
                p.hijoIzquierdo = t;
                t = p;

                lexema_anterior = miListaTokens[puntero].Lexema;
                ObtenerSiguienteToken();
                t.hijoDerecho = Termino();
                if (lexema_anterior == "+")
                {
                    t.miTipoOperacion = tipoOperacion.Suma;
                    codigo_p += "adi " + "\r\n";
                }
                else if (lexema_anterior == "-")
                {
                    t.miTipoOperacion = tipoOperacion.Resta;
                    codigo_p += "sbi " + "\r\n";
                }
            }

            return t;
        }

        public Nodo Termino()
        {
            Nodo t = Factor();
            while (miListaTokens[puntero].Lexema == "*" || miListaTokens[puntero].Lexema == "/")
            {
                Nodo p = NuevoNodoExpresion();
                string lexema_anterior = "";

                p.lexema = TokenActual.Lexema;
                p.miTipoExpresion = tipoExpresion.Aritmetica;
                p.hijoIzquierdo = t;
                t = p;

                lexema_anterior = miListaTokens[puntero].Lexema;
                ObtenerSiguienteToken();
                t.hijoDerecho = Factor();
                if (miListaTokens[puntero].Lexema == "*")
                {
                    codigo_p += "mpi " + "\r\n";
                    t.miTipoOperacion = tipoOperacion.Multiplicacion;
                }
                else if (miListaTokens[puntero].Lexema == "/")
                {
                    codigo_p += "div " + "\r\n";
                    t.miTipoOperacion = tipoOperacion.Division;
                }
            }

            return t;
        }

        public Nodo Factor()
        {
            Nodo t = new Nodo();
            if (TokenActual.Token == 100) // Identificador
            {
                t = NuevoNodoExpresion();
                t.lexema = TokenActual.Lexema;
                t.miTipoExpresion = tipoExpresion.Constante;
                t.miTipoValor = miTablaSimbolos.ObtenerTipoDato(TokenActual.Lexema);
                codigo_p += "lod " + TokenActual.Lexema + "\r\n";
                ObtenerSiguienteToken();
            }
            else if (TokenActual.Token == 101 || TokenActual.Token == 102)//Numero entero o deci
            {
                t = NuevoNodoExpresion();
                t.lexema = TokenActual.Lexema;
                t.miTipoExpresion = tipoExpresion.Constante;
                codigo_p += "ldc " + TokenActual.Lexema + "\r\n";
                if (TokenActual.Token == 101)
                {
                    t.miTipoValor = TipoDato.Entero;
                }
                else if (TokenActual.Token == 102)
                {
                    t.miTipoValor = TipoDato.Flotante;
                }
                ObtenerSiguienteToken();
            }
            return t;
        }
        #endregion

        #region Metodos para Crear Nodos
        public Nodo NuevoNodoExpresion()
        {
            Nodo t = new Nodo();
            t.hijoDerecho = new Nodo();
            t.hijoIzquierdo = new Nodo();
            t.miTipoExpresion = tipoExpresion.Vacio;
            t.miTipoSentencia = tipoSentencia.Vacio;
            return t;
        }

        public Nodo NuevoNodoSentencia()
        {
            Nodo t = new Nodo();
            t.Hermano = new Nodo();
            t.hijoIzquierdo = new Nodo();
            t.hijoCentro = new Nodo();
            t.hijoDerecho = new Nodo();
            t.miTipoExpresion = tipoExpresion.Vacio;
            t.miTipoSentencia = tipoSentencia.Vacio;
            return t;
        }
        #endregion
    }
}

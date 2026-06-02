using AgroShield.AlertEngine.Api.Data;
using AgroShield.AlertEngine.Api.Entities;
using AgroShield.AlertEngine.Api.Models;
using AgroShield.AlertEngine.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgroShield.AlertEngine.Api.Controllers;

[ApiController]
[Route("api/v1/alertas")]
public class AlertasController : ControllerBase
{
    private readonly IAlertCompositionService _compositionService;
    private readonly AgroShieldDbContext _context;
    private readonly ILogger<AlertasController> _logger;

    public AlertasController(
        IAlertCompositionService compositionService,
        AgroShieldDbContext context,
        ILogger<AlertasController> logger)
    {
        _compositionService = compositionService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Compoe o alerta de maior prioridade com base no terreno e metricas geo (RF-IA parcial / US-04).
    /// O backend Java envia o resultado para o servico Python TTS (mensagemParaFala).
    /// </summary>
    [HttpPost("compor")]
    [ProducesResponseType(typeof(AlertaComposicaoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AlertaComposicaoResponse>> Compor([FromBody] ComporAlertaRequest request)
    {
        try
        {
            if (request.TerrenoId <= 0)
                return BadRequest(new { message = "terrenoId deve ser maior que zero" });

            if (string.IsNullOrWhiteSpace(request.Nome))
                return BadRequest(new { message = "nome do terreno e obrigatorio" });

            if (request.Nome.Length > 120)
                return BadRequest(new { message = "nome do terreno deve ter no maximo 120 caracteres" });

            if (request.AreaTotalHectares <= 0)
                return BadRequest(new { message = "areaTotalHectares deve ser maior que zero" });

            if (request.AreaCultivoHectares < 0)
                return BadRequest(new { message = "areaCultivoHectares nao pode ser negativa" });

            var terreno = await _context.Terrenos.FindAsync(request.TerrenoId);
            if (terreno == null)
                return NotFound(new { message = $"Terreno com ID {request.TerrenoId} não encontrado" });

            var resultado = _compositionService.Compor(request);

            // salva no historico para poder exportar depois
            var historico = new HistoricoAlerta
            {
                TerrenoId = terreno.Id,
                Codigo = resultado.Codigo,
                Severidade = resultado.Severidade,
                MensagemParaFala = resultado.MensagemParaFala,
                MensagemTecnica = resultado.MensagemTecnica,
                AcaoRecomendada = resultado.AcaoRecomendada,
                NdviMedio = request.Geo?.NdviMedio,
                NdviZonaNorte = request.Geo?.NdviZonaNorte,
                NdviZonaSul = request.Geo?.NdviZonaSul,
                UmidadeRelativa = request.Geo?.UmidadeRelativa,
                DiasSemImagemSatelite = request.Geo?.DiasSemImagemSatelite,
                CriadoEm = DateTime.UtcNow
            };

            _context.HistoricoAlertas.Add(historico);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Alerta salvo: TerrenoId={TerrenoId}, Codigo={Codigo}, Severidade={Severidade}",
                terreno.Id, resultado.Codigo, resultado.Severidade);

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao compor alerta para TerrenoId={TerrenoId}", request.TerrenoId);
            return StatusCode(500, new { message = "Erro interno ao compor alerta" });
        }
    }

    [HttpGet("health")]
    public IActionResult Health() => Ok(new { status = "UP", service = "AgroShield.AlertEngine.Api" });
}


﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using FrontDesk.Core.Interfaces;
using FrontDesk.Core.ScheduleAggregate;
using FrontDesk.Core.ScheduleAggregate.Specifications;
using MediatR;
using Microsoft.Extensions.Logging;
using PluralsightDdd.SharedKernel.Interfaces;

namespace FrontDesk.Core.Handlers
{
  /// <summary>
  /// This handler responds to incoming messages saying a user has confirmed an appointment
  /// </summary>
  public class ConfirmAppointmentCommandHandler : IRequestHandler<ConfirmAppointmentCommand>
  {
    private readonly IRepository<Schedule> _scheduleRepository;
    private readonly IApplicationSettings _settings;
    private readonly ILogger<ConfirmAppointmentCommandHandler> _logger;

    public ConfirmAppointmentCommandHandler(IRepository<Schedule> scheduleRepository,
      IApplicationSettings settings,
      ILogger<ConfirmAppointmentCommandHandler> logger)
    {
      _scheduleRepository = scheduleRepository;
      _settings = settings;
      _logger = logger;
    }

    public async Task Handle(ConfirmAppointmentCommand confirmAppointmentConfirmedEvent,
      CancellationToken cancellationToken)
    {
      _logger.LogInformation($"Handling appointment confirmation: {confirmAppointmentConfirmedEvent.AppointmentId}");

      var scheduleSpec = new ScheduleForClinicAndDateWithAppointmentsSpec(_settings.ClinicId, _settings.TestDate);
      // Note: In this demo this only works for appointments scheduled on TestDate
      var schedule = (await _scheduleRepository.GetBySpecAsync(scheduleSpec));
      Guard.Against.Null(schedule, nameof(Schedule));

      var appointmentToConfirm = schedule.Appointments.FirstOrDefault(a => a.Id == confirmAppointmentConfirmedEvent.AppointmentId);

      appointmentToConfirm.Confirm(confirmAppointmentConfirmedEvent.DateOccurred);

      await _scheduleRepository.UpdateAsync(schedule);
    }
  }
}

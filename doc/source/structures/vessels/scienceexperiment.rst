.. _scienceexperimentmodule:

ScienceExperimentModule
=======================

The type of structures returned by kOS when querying a module that contains a science experiment.

Some of the science-related tasks are normally not available to kOS scripts. It is for
example possible to deploy a science experiment::

    SET P TO SHIP:PARTSNAMED("GooExperiment")[1].
    SET M TO P:GETMODULE("ModuleScienceExperiment").
    M:DOEVENT("observe mystery goo").

Hovewer, this results in a dialog being shown to the user. Only from that dialog it is possible
to reset the experiment or transmit the experiment results back to Kerbin.
:struct:`ScienceExperimentModule` structure introduces a few suffixes that allow the player
to perform all science-related tasks without any manual intervention::

    SET P TO SHIP:PARTSNAMED("GooExperiment")[0].
    SET M TO P:GETMODULE("ModuleScienceExperiment").
    M:DEPLOY.
    WAIT UNTIL M:HASDATA.
    M:TRANSMIT.


.. structure:: ScienceExperimentModule

    .. list-table::
        :header-rows: 1
        :widths: 2 1 4

        * - Suffix
          - Type
          - Description

        * - All suffixes of :struct:`PartModule`
          -
          - :struct:`ScienceExperimentModule` objects are a type of :struct:`PartModule`
        * - :meth:`DEPLOY()`
          -
          - Deploy and run the science experiment
        * - :meth:`RESET()`
          -
          - Reset this experiment if possible
        * - :meth:`TRANSMIT()`
          -
          - Transmit the scientific data back to Kerbin
        * - :meth:`DUMP()`
          -
          - Discard the data
        * - :attr:`INOPERABLE`
          - boolean
          - Is this experiment inoperable
        * - :attr:`RERUNNABLE`
          - boolean
          - Can this experiment be run multiple times
        * - :attr:`DEPLOYED`
          - boolean
          - Is this experiment deployed
        * - :attr:`HASDATA`
          - boolean
          - Does the experiment have scientific data

.. note::

    A :struct:`ScienceExperimentModule` is a type of :struct:`PartModule`, and therefore can use all the suffixes of :struct:`PartModule`.

.. method:: ScienceExperimentModule:DEPLOY()

    Call this method to deploy and run this science experiment. This method will fail if the experiment already contains scientific
    data or is inoperable.

.. method:: ScienceExperimentModule:RESET()

    Call this method to reset this experiment. This method will fail if the experiment is inoperable.

.. method:: ScienceExperimentModule:TRANSMIT()

    Call this method to transmit the results of the experiment back to Kerbin. This will render the experiment
    inoperable if it is not rerunnable. This method will fail if there is no data to send.

.. method:: ScienceExperimentModule:DUMP()

    Call this method to discard the data obtained as a result of running this experiment. This will render the experiment
    inoperable if it is not rerunnable.

.. attribute:: ScienceExperimentModule:INOPERABLE

    :access: Get only
    :type: boolean

    True if this experiment is no longer operable.

.. attribute:: ScienceExperimentModule:RERUNNABLE

    :access: Get only
    :type: boolean

    True if this experiment can be run multiple times.

.. attribute:: ScienceExperimentModule:DEPLOYED

    :access: Get only
    :type: boolean

    True if this experiment is deployed.

.. attribute:: ScienceExperimentModule:HASDATA

    :access: Get only
    :type: boolean

    True if this experiment has scientific data stored.

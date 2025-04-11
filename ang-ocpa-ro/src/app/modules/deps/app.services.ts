import { AppMenuManagementService } from 'src/app/services/api/app-menu-management.service';
import { AuthenticationService } from 'src/app/services/api/authentication.services';
import { ContentApiService } from 'src/app/services/api/content-api.service';
import { GeographyApiService } from 'src/app/services/api/geography-api.service';
import { MenuService } from 'src/app/services/api/menu.service';
import { MeteoApiService } from 'src/app/services/api/meteo-api.service';
import { ProtoneApiService } from 'src/app/services/api/protone-api.service';
import { RegisteredDeviceService } from 'src/app/services/api/registered-device.service';
import { UserTypeService } from 'src/app/services/api/user-type.service';
import { UserService } from 'src/app/services/api/user.service';
import { UtilityService } from 'src/app/services/api/utility.service';
import { FingerprintService } from 'src/app/services/fingerprint.service';
import { Iso3166HelperService } from 'src/app/services/iso3166-helper.service';
import { MessagePopupService } from 'src/app/services/message-popup.service';
import { SessionInformationService } from 'src/app/services/session-information.service';
import { TranslationInitService } from 'src/app/services/translation-init.service';

export const services = [
     // Services
     AuthenticationService,
     FingerprintService,
     SessionInformationService,
     TranslationInitService,
     GeographyApiService,
     Iso3166HelperService,
     UserTypeService,
     UserService,
     RegisteredDeviceService,
     MenuService,
     ProtoneApiService,
     MeteoApiService,
     ContentApiService,
     AppMenuManagementService,
     MessagePopupService,
     UtilityService
];
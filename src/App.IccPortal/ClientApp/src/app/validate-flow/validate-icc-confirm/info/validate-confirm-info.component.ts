// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

import { Component, OnInit } from '@angular/core';
import {CarousselImage} from '../../../models/caroussel-image';

@Component({
  selector: 'app-validate-step4',
  templateUrl: './validate-confirm-info.component.html',
  styleUrls: ['./validate-confirm-info.component.scss']
})
export class ValidateConfirmInfoComponent implements OnInit {
  images: Array<CarousselImage> = [
    {
      text: 'Scroll naar beneden',
      image: 'assets/images/new/4.svg',
      device: true
    },
    {
      text: 'Druk op ‘Ga door’ om de codes te delen',
      image: 'assets/images/new/5.svg',
      device: true
    },
    {
      text: 'Geef toestemming',
      image: 'assets/images/new/6.svg',
      device: true
    }
  ];
  constructor() { }

  ngOnInit(): void {
  }

}
